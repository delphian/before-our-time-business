﻿using BeforeOurTime.Business.Apis;
using BeforeOurTime.Business.JsEvents;
using BeforeOurTime.Business.JsFunctions;
using BeforeOurTime.Business.Logs;
using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Repository.Dbs.EF;
using BeforeOurTime.Repository.Models;
using BeforeOurTime.Repository.Models.Accounts;
using BeforeOurTime.Repository.Models.Accounts.Authentication.Providers;
using BeforeOurTime.Repository.Models.Accounts.Authorization;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Messages;
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
            ServiceProvider.CreateScope().ServiceProvider.GetService<IApi>().DataReset().DataInstall(
                Configuration["Setup"]);
            // Setup automatic message deliver and Tick counter for items
            var tickTimer = new System.Threading.Timer(Tick, null, 0, 1000);
            var deliverTimer = new System.Threading.Timer(DeliverMessages, null, 0, 500);
            ListenToTerminals(ServiceProvider.CreateScope().ServiceProvider);
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
                .AddScoped<ICharacterRepo, CharacterRepo>()
                .AddScoped<IMessageRepo, MessageRepo>()
                .AddScoped<IRepository<AuthorizationRole>, Repository<AuthorizationRole>>()
                .AddScoped<IRepository<AuthorizationGroup>, Repository<AuthorizationGroup>>()
                .AddScoped<IRepository<AuthorizationGroupRole>, Repository<AuthorizationGroupRole>>()
                .AddScoped<IRepository<AuthorizationAccountGroup>, Repository<AuthorizationAccountGroup>>()
                .AddScoped<IRepository<AuthenticationBotMeta>, Repository<AuthenticationBotMeta>>()
                .AddScoped<IJsEventManager, JsEventManager>()
                .AddScoped<IApi, Api>()
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
                terminal.OnMessageToServer += delegate (Terminal xterminal, string message)
                {
                    lock (thisLock)
                    {
                        var itemRepo = serviceProvider.GetService<IItemRepo<Item>>();
                        var api = serviceProvider.GetService<IApi>();
                        var from = itemRepo.Read(new List<Guid>() { terminal.AvatarId }).First();
                        var clientMessage = new Message()
                        {
                            Version = ItemVersion.Alpha,
                            Type = MessageType.EventTerminalInput,
                            From = from,
                            FromId = from.Id,
                            Value = JsonConvert.SerializeObject(new OnTerminalInput() {
                                Terminal = terminal,
                                Raw = message
                            })
                        };
                        api.SendMessage(clientMessage, itemRepo.Read());
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
                var api = ServiceProvider.GetService<IApi>();
                var gameItem = itemRepo.Read(new List<Guid>() { new Guid("487a7282-0cad-4081-be92-83b14671fc23") }).First();
                var tickMessage = new Message()
                {
                    Version = ItemVersion.Alpha,
                    Type = MessageType.EventTick,
                    From = gameItem,
                    Value = "{}"
                };
                //api.SendMessage(tickMessage, itemRepo.Read());
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
                var jsEventManager = ServiceProvider.GetService<IJsEventManager>();
                var parser = new Jint.Parser.JavaScriptParser();
                var jsEngine = new Engine();
                // Create script global functions
                var jsFunctionManager = new JsFunctionManager(Configuration, ServiceProvider);
                var jsEvents = jsEventManager.GetJsEventRegistrations();
                // Get messages
                List<Message> messages = messageRepo.Read();
                messageRepo.Delete();
                // Deliver message to each recipient
                foreach (Message message in messages)
                {
                    try
                    {
                        var me = itemRepo.Read(new List<Guid>() { message.ToId }).First();
                        var jsProgram = parser.Parse(me.Script.Trim());
                        var jsEventHandler = jsEvents.Where(x => x.MessageType == message.Type).Select(x => x.JsFunction).FirstOrDefault();
                        var jsArgumentType = jsEvents.Where(x => x.MessageType == message.Type).Select(x => x.JsArgument).FirstOrDefault();
                        if (jsProgram.FunctionDeclarations.Any(x => x.Id.Name == jsEventHandler))
                        {
                            jsFunctionManager.AddJsFunctions(jsEngine);
                            jsEngine
                                .SetValue("me", me)
                                .SetValue("_data", JsonConvert.SerializeObject(JsonConvert.DeserializeObject(me.Data)))
                                .Execute("var data = JSON.parse(_data);")
                                .Execute(me.Script)
                                .Invoke(
                                    jsEventHandler,
                                    JsonConvert.DeserializeObject(message.Value, jsArgumentType)
                                );
                            // Save changes to item data
                            me.Data = JsonConvert.SerializeObject(jsEngine.GetValue("data").ToObject());
                            itemRepo.Update(new List<Item>() { me });
                        }
                        else
                        {
                            // logger.LogError(message.To.Id + ": No js callback for: " + jsEvents[message.Type].Function);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("script failed: " + message.ToId + " " + ex.Message);
                    }
                }
            }
        }
    }
}
