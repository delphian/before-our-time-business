#define DEBUG

using BeforeOurTime.Business.Apis.Messages;
using BeforeOurTime.Business.Servers;
using BeforeOurTime.Models.Messages.Requests;
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
using BeforeOurTime.Business.Modules;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Logs;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Business.Logs;
using BeforeOurTime.Models.Modules.Terminal.Managers;
using BeforeOurTime.Models.Modules.Terminal.Models;
using BeforeOurTime.Business.Modules.Terminal.Managers;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Core.Models.Data;
using BeforeOurTime.Models.Modules.Terminal.Models.Data;

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
            // Setup automatic message deliver and Tick counter for items
            var masterCts = new CancellationTokenSource();
            // Start servers
            ServiceProvider.GetService<IBotLogger>().LogInformation($"Starting servers...");
            Servers = BuildServerList(ServiceProvider);
            Servers.ForEach(delegate (IServer server)
            {
                server.Start();
            });
            ServiceProvider.GetService<IBotLogger>().LogInformation($"All servers started");
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
                .AddSingleton<IBotLogger>(new FileLogger(configuration))
                .AddSingleton<IConfiguration>(configuration)
                .AddLogging()
                .AddScoped<IMessageManager, MessageManager>()
                .AddScoped<IModuleManager, ModuleManager>();
        }
        /// <summary>
        /// Use reflection to register all classes which will be initialised as client servers
        /// </summary>
        /// <param name="api">Before Our Time API</param>
        /// <returns></returns>
        private static List<IServer> BuildServerList(IServiceProvider services)
        {
            var serverList = new List<IServer>();
            var interfaceType = typeof(IServer);
            serverList = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x => (IServer)Activator.CreateInstance(x, services, Configuration))
                .ToList();
            return serverList;
        }
        /// <summary>
        /// Monitor terminal connections and forward messages
        /// </summary>
        private static void ListenToTerminals()
        {
            var moduleManager = ServiceProvider.GetService<IModuleManager>();
            var terminalManager = moduleManager.GetManager<ITerminalManager>();
            var itemManager = moduleManager.GetManager<IItemManager>();
            ((TerminalManager)terminalManager).OnTerminalCreated += delegate (ITerminal terminal)
            {
                ((Terminal)terminal).OnMessageToServer += delegate (ITerminal xterminal, IRequest request)
                {
                    lock (thisLock)
                    {
                        IResponse response = new Response()
                        {
                            _requestInstanceId = request.GetRequestInstanceId(),
                            _responseSuccess = false
                        };
                        // Get terminal's item, or assign it a ghost
                        Item origin = (terminal.GetPlayerId() != null) ?
                            itemManager.Read(terminal.GetPlayerId().Value) :
                            new Item() {
                                Type = ItemType.Ghost,
                                Data = new List<IItemData>() { new TerminalData() {
                                    TerminalId = terminal.GetId(),
                                    Terminal = terminal
                                } }
                            };
                        response = ServiceProvider.GetService<IModuleManager>()
                            .HandleMessage(request, origin, response);
                        return response;
                    }
                };
            };
        }
    }
}
