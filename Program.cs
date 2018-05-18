#define DEBUG

using BeforeOurTime.Business.Apis;
using BeforeOurTime.Business.Apis.Accounts;
using BeforeOurTime.Business.Apis.Items;
using BeforeOurTime.Business.Apis.Items.Characters;
using BeforeOurTime.Business.Apis.Items.Games;
using BeforeOurTime.Business.Apis.Items.Locations;
using BeforeOurTime.Business.Apis.Messages;
using BeforeOurTime.Business.Apis.Scripts;
using BeforeOurTime.Business.Apis.Scripts.Engines;
using BeforeOurTime.Business.Apis.Scripts.Libraries;
using BeforeOurTime.Business.Logs;
using BeforeOurTime.Business.Models.ScriptCallbacks.Arguments;
using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Repository.Dbs.EF;
using BeforeOurTime.Repository.Models;
using BeforeOurTime.Repository.Models.Accounts;
using BeforeOurTime.Repository.Models.Accounts.Authentication.Providers;
using BeforeOurTime.Repository.Models.Accounts.Authorization;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Scripts.Callbacks;
using BeforeOurTime.Repository.Models.Scripts.Interfaces;
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
            // Setup automatic message deliver and Tick counter for items
            var tickTimer = new System.Threading.Timer(Tick, null, 0, Int32.Parse(Configuration.GetSection("Timing")["Tick"]));
            var deliverTimer = new System.Threading.Timer(DeliverMessages, null, 0, Int32.Parse(Configuration.GetSection("Timing")["Delivery"]));
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
                .AddScoped<IItemCharacterRepo, CharacterRepo>()
                .AddScoped<IMessageRepo, MessageRepo>()
                .AddScoped<IRepository<AuthorizationRole>, Repository<AuthorizationRole>>()
                .AddScoped<IAuthorGroupRepo, AuthorGroupRepo>()
                .AddScoped<IRepository<AuthorizationGroupRole>, Repository<AuthorizationGroupRole>>()
                .AddScoped<IRepository<AuthorizationAccountGroup>, Repository<AuthorizationAccountGroup>>()
                .AddScoped<IRepository<AuthenticationBotMeta>, Repository<AuthenticationBotMeta>>()
                .AddScoped<IScriptCallbackRepo, ScriptCallbackRepo>()
                .AddScoped<IScriptInterfaceRepo, ScriptInterfaceRepo>()
                // Main environment interface API
                .AddScoped<IScriptEngine, JsScriptEngine>()
                .AddScoped<IAccountManager, AccountManager>()
                .AddScoped<IScriptManager, ScriptManager>()
                .AddScoped<IMessageManager, MessageManager>()
                .AddScoped<IItemManager, ItemManager>()
                .AddScoped<IItemGameManager, ItemGameManager>()
                .AddScoped<IItemLocationManager, ItemLocationManager>()
                .AddScoped<IItemCharacterManager, ItemCharacterManager>()
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
                        var callback = api.GetScriptManager().GetCallbackDefinition("onTerminalInput");
                        var clientMessage = new Message()
                        {
                            Callback = callback,
                            Sender = from,
                            Package = JsonConvert.SerializeObject(new ArgTerminalInput() {
                                Terminal = terminal,
                                Raw = message
                            })
                        };
                        api.GetMessageManager().SendMessage(clientMessage, itemRepo.Read());
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
                    Callback = api.GetScriptManager().GetCallbackDefinition("onTick"),
                    Sender = gameItem,
                    Package = "{}"
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
                var ScriptEngine = ServiceProvider.GetService<IScriptEngine>();
                // Create script global functions
                var jsFunctionManager = new JsFunctionManager(Configuration, ServiceProvider);
                // Get messages
                List<Message> messages = messageRepo.Read();
                messageRepo.Delete();
                // Deliver message to each recipient
                foreach (Message message in messages)
                {
#if !DEBUG
                    try
                    {
#endif
                        var me = itemRepo.Read(new List<Guid>() { message.RecipientId }).First();
                        if (ScriptEngine.GetFunctionDeclarations(me.Script.Trim()).Contains(message.Callback.FunctionName))
                        {
                            jsFunctionManager.AddJsFunctions(ScriptEngine);
                            ScriptEngine
                                .SetValue("me", me)
                                .SetValue("_data", JsonConvert.SerializeObject(JsonConvert.DeserializeObject(me.Data)))
                                .Execute("var data = JSON.parse(_data);")
                                .Execute(me.Script)
                                .Invoke(
                                    message.Callback.FunctionName,
                                    JsonConvert.DeserializeObject(message.Package, Type.GetType(message.Callback.ArgumentType))
                                );
                            // Save changes to item data
                            me.Data = JsonConvert.SerializeObject(ScriptEngine.GetValue("data"));
                            itemRepo.Update(new List<Item>() { me });
                        }
                    }
#if !DEBUG
                    catch (Exception ex)
                    {
                        logger.LogError("script failed: " + message.ToId + " " + ex.Message);
                    }
                }
#endif
            }
        }
    }
}
