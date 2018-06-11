#define DEBUG

using BeforeOurTime.Business.Apis;
using BeforeOurTime.Business.Apis.Accounts;
using BeforeOurTime.Business.Apis.IO;
using BeforeOurTime.Business.Apis.Items;
using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Business.Apis.Items.Attributes.Interfaces;
using BeforeOurTime.Business.Apis.Messages;
using BeforeOurTime.Business.Apis.Scripts;
using BeforeOurTime.Business.Apis.Scripts.Delegates.OnTerminalInput;
using BeforeOurTime.Business.Apis.Scripts.Engines;
using BeforeOurTime.Business.Apis.Scripts.Libraries;
using BeforeOurTime.Business.Logs;
using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Repository.Dbs.EF;
using BeforeOurTime.Repository.Dbs.EF.Items;
using BeforeOurTime.Repository.Dbs.EF.Items.Attributes;
using BeforeOurTime.Repository.Models;
using BeforeOurTime.Repository.Models.Accounts;
using BeforeOurTime.Repository.Models.Accounts.Authentication.Providers;
using BeforeOurTime.Repository.Models.Accounts.Authorization;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Attributes.Repos;
using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Messages.Data;
using BeforeOurTime.Repository.Models.Messages.Requests;
using BeforeOurTime.Repository.Models.Scripts.Delegates;
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
            Console.WriteLine("Main thread: " + Thread.CurrentThread.ManagedThreadId.ToString());
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
            ListenToTerminals(ServiceProvider);
            // Wait for user input
            Console.WriteLine("Hit 'q' and enter to abort\n");
            string clientInput = Console.ReadLine();
            while (clientInput != "q")
            {
                clientInput = Console.ReadLine();
            }
            Servers.Telnet.TelnetManager.TelnetServer.Stop();
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
                .AddDbContext<BaseContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Scoped)
                .AddLogging()
                // Repositories
                .AddScoped<IAccountRepo, AccountRepo>()
                .AddScoped<IMessageRepo, MessageRepo>()
                .AddScoped<IRepository<AuthorizationRole>, Repository<AuthorizationRole>>()
                .AddScoped<IAuthorGroupRepo, AuthorGroupRepo>()
                .AddScoped<IRepository<AuthorizationGroupRole>, Repository<AuthorizationGroupRole>>()
                .AddScoped<IRepository<AuthorizationAccountGroup>, Repository<AuthorizationAccountGroup>>()
                .AddScoped<IRepository<AuthenticationBotMeta>, Repository<AuthenticationBotMeta>>()
                .AddScoped<IScriptInterfaceRepo, ScriptInterfaceRepo>()
                // Repositories (Items)
                .AddScoped<IItemRepo, ItemRepo>()
                .AddScoped<IAttributePlayerRepo, AttributePlayerRepo>()
                .AddScoped<IAttributeGameRepo, AttributeGameRepo>()
                .AddScoped<IAttributeLocationRepo, AttributeLocationRepo>()
                .AddScoped<IAttributePhysicalRepo, AttributePhysicalRepo>()
                .AddScoped<IAttributeExitRepo, AttributeExitRepo>()
                // Main environment interface API
                .AddScoped<IScriptEngine, JsScriptEngine>()
                .AddScoped<IAccountManager, AccountManager>()
                .AddScoped<IScriptManager, ScriptManager>()
                .AddScoped<IMessageManager, MessageManager>()
                .AddScoped<IIOManager, IOManager>()
                // Items and item attributes
                .AddScoped<IItemManager, ItemManager>()
                .AddScoped<IAttributeGameManager, AttributeGameManager>()
                .AddScoped<IAttributeLocationManager, AttributeLocationManager>()
                .AddScoped<IAttributePlayerManager, AttributePlayerManager>()
                .AddScoped<IAttributePhysicalManager, AttributePhysicalManager>()
                .AddScoped<IAttributeExitManager, AttributeExitManager>()
                .AddScoped<IApi, Api>()
                .AddScoped<ITerminalManager, TerminalManager>();
        }
        /// <summary>
        /// Monitor terminal connections and forward messages
        /// </summary>
        /// <param name="serviceProvider"></param>
        private static void ListenToTerminals(IServiceProvider serviceProvider)
        {
            Console.WriteLine("Terminal thread: " + Thread.CurrentThread.ManagedThreadId.ToString());
            var terminalManager = serviceProvider.GetService<ITerminalManager>();
            var telnetServer = new Servers.Telnet.TelnetManager(serviceProvider);
            var api = serviceProvider.GetService<IApi>();
            ((TerminalManager)terminalManager).OnTerminalCreated += delegate (Terminal terminal)
            {
                terminal.OnMessageToServer += delegate (Terminal xterminal, IRequest terminalRequest)
                {
                    lock (thisLock)
                    {
                        api.GetIOManager().HandleRequest(api, terminal, terminalRequest);
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
                var api = ServiceProvider.GetService<IApi>();
                var game = api.GetAttributeManager<IAttributeGameManager>().GetDefaultGame();
                var onTickDelegate = api.GetScriptManager().GetDelegateDefinition("onTick");
                var itemRecipients = api.GetItemManager().Read(onTickDelegate);
                var tickMessage = new SavedMessage()
                {
                    DelegateId = onTickDelegate.GetId(),
                    SenderId = game.Id,
                    Package = "{}"
                };
                api.GetMessageManager().SendMessage(tickMessage, itemRecipients);
            }
        }
        /// <summary>
        /// Deliver messages to their recipient items and execute each item script
        /// </summary>
        /// <param name="o"></param>
        public static void DeliverMessages(object o)
        {
            lock (thisLock)
            {
                var logger = ServiceProvider.GetService<ILogger>();
                var messageRepo = ServiceProvider.GetService<IMessageRepo>();
                var api = ServiceProvider.GetService<IApi>();
                // Create script global functions
                var jsFunctionManager = new JsFunctionManager(Configuration, ServiceProvider);
                // Get messages
                List<SavedMessage> messages = messageRepo.Read();
                messageRepo.Delete();
                // Deliver message to each recipient
                foreach (SavedMessage message in messages)
                {
#if !DEBUG
                    try
                    {
#endif
                        var item = api.GetItemManager().Read(message.RecipientId);
                        List<IAttributeManager> attributeManagers = api.GetAttributeManagers(item);
                        // Hand off message deliver to each item's manager code
                        attributeManagers.ForEach(delegate (IAttributeManager attributeManager)
                        {
                            // TODO : This should probably just add items to jsFunctionManager
                            // and then execute the script once instead of each manager executing the script
                            attributeManager.DeliverMessage(message, item, jsFunctionManager);
                        });
#if !DEBUG
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("script failed: " + message.ToId + " " + ex.Message);
                    }
#endif
                }
            }
        }
    }
}
