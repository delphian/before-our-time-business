#define DEBUG

using BeforeOurTime.Business.Apis;
using BeforeOurTime.Business.Apis.Accounts;
using BeforeOurTime.Business.Apis.Items;
using BeforeOurTime.Business.Apis.Logs;
using BeforeOurTime.Business.Apis.Messages;
using BeforeOurTime.Business.Apis.Scripts;
using BeforeOurTime.Business.Apis.Scripts.Engines;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Business.Servers;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Repository.Dbs.EF;
using BeforeOurTime.Repository.Dbs.EF.Items;
using BeforeOurTime.Repository.Dbs.EF.Items.Attributes;
using BeforeOurTime.Repository.Models;
using BeforeOurTime.Repository.Models.Accounts;
using BeforeOurTime.Models.Accounts.Authentication.Providers;
using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Messages.Data;
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
using BeforeOurTime.Business.Apis.Items.Attributes.Locations;
using BeforeOurTime.Business.Apis.Items.Attributes.Players;
using BeforeOurTime.Business.Apis.Items.Attributes.Exits;
using BeforeOurTime.Business.Apis.Items.Attributes.Games;
using BeforeOurTime.Business.Apis.Items.Attributes.Physicals;
using BeforeOurTime.Business.Apis.Items.Attributes.Characters;
using BeforeOurTime.Models.Items.Attributes.Exits;
using BeforeOurTime.Models.Items.Attributes.Physicals;
using BeforeOurTime.Models.Items.Attributes.Locations;
using BeforeOurTime.Models.Items.Attributes.Games;
using BeforeOurTime.Models.Items.Attributes.Characters;
using BeforeOurTime.Models.Items.Attributes.Players;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models;

namespace BeforeOurTime.Business
{
    class Program
    {
        public static IConfiguration Configuration { set; get; }
        public static IServiceProvider ServiceProvider { set; get; }
        public static List<IServer> Servers { set; get; }
        public static Object thisLock = new Object();
        public static void Main(string[] args)
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
            var api = ServiceProvider.GetService<IApi>();
            // Setup automatic message deliver and Tick counter for items
            var masterCts = new CancellationTokenSource();
            var tickTask = api.TickAsync(Convert.ToInt32(Configuration.GetSection("Timing")["Tick"]), masterCts.Token);
            var deliverTask = api.DeliverMessagesAsync(Int32.Parse(Configuration.GetSection("Timing")["Delivery"]), masterCts.Token, Configuration, ServiceProvider);
            // Start servers
            ServiceProvider.GetService<ILogger>().LogInformation($"Starting servers...");
            Servers = BuildServerList(ServiceProvider.GetService<IApi>());
            Servers.ForEach(delegate (IServer server)
            {
                server.Start();
            });
            ServiceProvider.GetService<ILogger>().LogInformation($"All servers started");
            ListenToTerminals();
            // Wait for user input
            Console.WriteLine("Ready! (Hit 'q' and enter to abort console)");
            string clientInput = Console.ReadLine();
            while (clientInput != "q")
            {
                clientInput = Console.ReadLine();
            }
            // Shutdown everything
            masterCts.Cancel();
            // Stop servers
            Servers.ForEach(delegate (IServer server)
            {
                server.Stop();
            });
        }
        /// <summary>
        /// Setup services
        /// </summary>
        private static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            // Setup services
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services
                .AddSingleton<ILogger>(new FileLogger(configuration))
                .AddSingleton<IConfiguration>(configuration)
                .AddDbContext<BaseContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Scoped)
                .AddLogging()
                // Repositories
                .AddScoped<IAccountRepo, AccountRepo>()
                .AddScoped<IMessageRepo, MessageRepo>()
                .AddScoped<IRepository<AuthenticationBotMeta>, Repository<AuthenticationBotMeta>>()
                .AddScoped<IScriptInterfaceRepo, ScriptInterfaceRepo>()
                // Repositories (Items)
                .AddScoped<IItemRepo, ItemRepo>()
                .AddScoped<IPlayerAttributeRepo, PlayerAttributeRepo>()
                .AddScoped<ICharacterAttributeRepo, CharacterAttributeRepo>()
                .AddScoped<IAttributeGameRepo, AttributeGameRepo>()
                .AddScoped<IAttributeLocationRepo, LocationAttributeRepo>()
                .AddScoped<IAttributePhysicalRepo, AttributePhysicalRepo>()
                .AddScoped<IAttributeExitRepo, AttributeExitRepo>()
                // Main environment interface API
                .AddScoped<IScriptEngine, JsScriptEngine>()
                .AddScoped<IAccountManager, AccountManager>()
                .AddScoped<IScriptManager, ScriptManager>()
                .AddScoped<IMessageManager, MessageManager>()
                .AddScoped<ITerminalManager, TerminalManager>()
                // Items and item attributes
                .AddScoped<IItemManager, ItemManager>()
                .AddScoped<IGameAttributeManager, GameAttributeManager>()
                .AddScoped<ILocationAttributeManager, LocationAttributeManager>()
                .AddScoped<IPlayerAttributeManager, PlayerAttributeManager>()
                .AddScoped<ICharacterAttributeManager, CharacterAttributeManager>()
                .AddScoped<IPhysicalAttributeManager, PhysicalAttributeManager>()
                .AddScoped<IExitAttributeManager, ExitAttributeManager>()
                .AddScoped<IApi, Api>();
        }
        /// <summary>
        /// Use reflection to register all classes which will be initialised as client servers
        /// </summary>
        /// <param name="api">Before Our Time API</param>
        /// <returns></returns>
        private static List<IServer> BuildServerList(IApi api)
        {
            var serverList = new List<IServer>();
            var interfaceType = typeof(IServer);
            serverList = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x => (IServer)Activator.CreateInstance(x, api, Configuration))
                .ToList();
            return serverList;
        }
        /// <summary>
        /// Monitor terminal connections and forward messages
        /// </summary>
        private static void ListenToTerminals()
        {
            var api = ServiceProvider.GetService<IApi>();
            ((TerminalManager)ServiceProvider.GetService<ITerminalManager>()).OnTerminalCreated += delegate (Terminal terminal)
            {
                terminal.OnMessageToServer += delegate (Terminal xterminal, IRequest request)
                {
                    lock (thisLock)
                    {
                        var response = api.GetMessageManager().HandleRequest(api, terminal, request);
                        return response;
                    }
                };
            };
        }
    }
}
