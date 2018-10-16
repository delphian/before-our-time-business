#define DEBUG

using BeforeOurTime.Business.Apis;
using BeforeOurTime.Business.Apis.Logs;
using BeforeOurTime.Business.Apis.Messages;
using BeforeOurTime.Business.Apis.Terminals;
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
using BeforeOurTime.Models.Terminals;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Logs;

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
//            var tickTask = api.TickAsync(Convert.ToInt32(Configuration.GetSection("Timing")["Tick"]), masterCts.Token);
//            var deliverTask = api.DeliverMessagesAsync(Int32.Parse(Configuration.GetSection("Timing")["Delivery"]), masterCts.Token, Configuration, ServiceProvider);
            // Start servers
            ServiceProvider.GetService<IBotLogger>().LogInformation($"Starting servers...");
            Servers = BuildServerList(ServiceProvider.GetService<IApi>());
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
                .AddScoped<ITerminalManager, TerminalManager>()
                .AddScoped<IModuleManager, ModuleManager>()
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
            ((TerminalManager)ServiceProvider.GetService<ITerminalManager>()).OnTerminalCreated += delegate (ITerminal terminal)
            {
                ((Terminal)terminal).OnMessageToServer += delegate (ITerminal xterminal, IRequest request)
                {
                    lock (thisLock)
                    {
                        var response = api.GetMessageManager().HandleRequest(api, terminal, request);
                        response = api.GetModuleManager().HandleMessage(request, api, terminal, response);
                        return response;
                    }
                };
            };
        }
    }
}
