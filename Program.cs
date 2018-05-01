﻿using BeforeOurTime.Business.Apis;
using BeforeOurTime.Business.JsFunctions;
using BeforeOurTime.Business.JsMessageBody;
using BeforeOurTime.Business.Logs;
using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Repository.Dbs.EF;
using BeforeOurTime.Repository.Models;
using BeforeOurTime.Repository.Models.Accounts;
using BeforeOurTime.Repository.Models.Accounts.Authentication.Providers;
using BeforeOurTime.Repository.Models.Accounts.Authorization;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Messages.Events.Maps;
using BeforeOutTime.Repository.Dbs.EF;
using Jint;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace BeforeOurTime.Business
{
    class Program
    {
        public static IConfigurationRoot Configuration { set; get; }
        public static IServiceProvider ServiceProvider { set; get; }
        public static IApi Api { set; get; }
        public static Object thisLock = new Object();
        static void Main(string[] args)
        {
            // Setup configuration
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
            IServiceCollection services = new ServiceCollection();
            ConfigureServices(Configuration, services);
            ServiceProvider = services.BuildServiceProvider();
            Api = new Api(Configuration, ServiceProvider);
            new Setups.Setup(Configuration, ServiceProvider).Install();
            // Setup automatic message deliver and Tick counter for items
            var tickTimer = new System.Threading.Timer(Tick, null, 0, 1000);
            var deliverTimer = new System.Threading.Timer(DeliverMessages, null, 0, 500);
            ListenToTerminals(ServiceProvider);
            // Wait for user input
            Console.WriteLine("Hit 'q' and enter to abort\n");
            string clientInput = Console.ReadLine();
            while (clientInput != "q")
            {
                clientInput = Console.ReadLine();
            }
            Servers.Telnet.Server.s.stop();
        }
        /// <summary>
        /// Setup services
        /// </summary>
        private static void ConfigureServices(IConfigurationRoot configuration, IServiceCollection services)
        {
            // Setup services
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services
                .AddSingleton<ILogger>(new FileLogger())
                .AddDbContext<BaseContext>(options => options.UseSqlite(connectionString), ServiceLifetime.Scoped)
                .AddLogging()
                .AddScoped<IAccountRepo, AccountRepo>()
                .AddScoped<IItemRepo<Item>, ItemRepo<Item>>()
                .AddScoped<IMessageRepo, MessageRepo>()
                .AddScoped<IRepository<AuthorizationRole>, Repository<AuthorizationRole>>()
                .AddScoped<IRepository<AuthorizationGroup>, Repository<AuthorizationGroup>>()
                .AddScoped<IRepository<AuthorizationGroupRole>, Repository<AuthorizationGroupRole>>()
                .AddScoped<IRepository<AuthorizationAccountGroup>, Repository<AuthorizationAccountGroup>>()
                .AddScoped<IRepository<AuthenticationBotMeta>, Repository<AuthenticationBotMeta>>()
                .AddSingleton<ITerminalManager, TerminalManager>();
        }
        /// <summary>
        /// Monitor terminal connections and forward messages
        /// </summary>
        /// <param name="serviceProvider"></param>
        private static void ListenToTerminals(IServiceProvider serviceProvider)
        {
            var terminalManager = serviceProvider.GetService<ITerminalManager>();
            var telnetServer = new Servers.Telnet.Server(serviceProvider);
            ((TerminalManager)terminalManager).OnTerminalCreated += delegate (Terminal terminal)
            {
                terminal.OnMessageToServer += delegate (Guid terminalId, string message)
                {
                    lock (thisLock)
                    {
                        var itemRepo = serviceProvider.GetService<IItemRepo<Item>>();
                        var gameItem = itemRepo.ReadUuid(new List<Guid>() { terminal.ItemUuid }).First();
                        var clientMessage = new Message()
                        {
                            Version = ItemVersion.Alpha,
                            Type = MessageType.EventClientInput,
                            From = gameItem,
                            Value = JsonConvert.SerializeObject(new BodyEventClientInput() { Raw = message })
                        };
                        Api.SendMessage(clientMessage, itemRepo.Read());
                    }
                };
            };
        }
        /// <summary>
        /// Execute all item scripts that desire a regular periodic event
        /// </summary>
        /// <param name="o"></param>
        private static void Tick(object o)
        {
            lock(thisLock)
            {
                var itemRepo = ServiceProvider.GetService<IItemRepo<Item>>();
                var gameItem = itemRepo.ReadUuid(new List<Guid>() { new Guid("487a7282-0cad-4081-be92-83b14671fc23") }).First();
                var tickMessage = new Message()
                {
                    Version = ItemVersion.Alpha,
                    Type = MessageType.EventTick,
                    From = gameItem,
                    Value = "{}"
                };
                Api.SendMessage(tickMessage, itemRepo.Read());
            }
        }
        /// <summary>
        /// Deliver messages to their recipient items and execute each item script
        /// </summary>
        /// <param name="o"></param>
        public static void DeliverMessages(object o)
        {
            lock(thisLock)
            {
                var logger = ServiceProvider.GetService<ILogger>();
                var itemRepo = ServiceProvider.GetService<IItemRepo<Item>>();
                var messageRepo = ServiceProvider.GetService<IMessageRepo>();
                var parser = new Jint.Parser.JavaScriptParser();
                var jsEngine = new Engine();
                // Javascript onEvent function name mapping to message type
                var jsEvents = MapMessageHandlers.GetEventJsMapping();
                // Create script global functions
                GetJsFunctions(Configuration, ServiceProvider, Api, jsEngine);
                // Get messages
                List<Message> messages = messageRepo.Read();
                messageRepo.Delete();
                // Deliver message to each recipient
                foreach (Message message in messages)
                {
                    try
                    {
                        var jsProgram = parser.Parse(message.To.Script.Trim());
                        if (jsProgram.FunctionDeclarations.Any(x => x.Id.Name == jsEvents[message.Type].Function))
                        {
                            jsEngine
                                .SetValue("me", message.To)
                            .SetValue("_data", JsonConvert.SerializeObject(JsonConvert.DeserializeObject(message.To.Data)))
                            .Execute("var data = JSON.parse(_data);")
                            .Execute(message.To.Script)
                            .Invoke(
                                jsEvents[message.Type].Function,
                                JsonConvert.DeserializeObject(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(message.Value)))
                            );
                            // Save changes to item data
                            message.To.Data = JsonConvert.SerializeObject(jsEngine.GetValue("data").ToObject());
                            itemRepo.Update(new List<Item>() { message.To });
                            // Forward message to terminal
                            var terminalManager = ServiceProvider.GetService<ITerminalManager>();
                            var terminal = terminalManager.GetTerminals().Where(x => x.ItemUuid == message.To.Uuid).FirstOrDefault();
                            if (terminal != null)
                            {
                                terminal.SendToTerminal(message.Value);
                            }
                        }
                        else
                        {
                            logger.LogError(message.To.Uuid + ": No js callback for: " + jsEvents[message.Type].Function);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("script failed: " + message.To.Uuid + " " + ex.Message);
                    }
                }
            }
        }
        /// <summary>
        /// Create all javascript functions for scripts to use
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="api"></param>
        /// <param name="jsEngine"></param>
        public static void GetJsFunctions(
            IConfigurationRoot configuration, 
            IServiceProvider serviceProvider,
            IApi api,
            Engine jsEngine)
        {
            var interfaceType = typeof(IJsFunc);
            var jsFuncClasses = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x => Activator.CreateInstance(x, configuration, serviceProvider, api, jsEngine))
                .ToList();
            jsFuncClasses
                .ForEach(x => ((IJsFunc)x).AddFunctions());
        }
    }
}