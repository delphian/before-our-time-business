using BeforeOurTime.Business.Apis;
using BeforeOurTime.Business.JsFunctions;
using BeforeOurTime.Business.JsMessageBody;
using BeforeOurTime.Business.Logs;
using BeforeOurTime.Repository.Dbs.EF;
using BeforeOurTime.Repository.Models;
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
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            // Setup services
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            var db = new BaseContext((new DbContextOptionsBuilder<BaseContext>()).UseSqlite(connectionString).Options);
            // Setup service provider
            ServiceProvider = new ServiceCollection()
                .AddSingleton<ILogger>(new FileLogger())
                .AddLogging()
                .AddDbContext<BaseContext>(options => options.UseSqlite(connectionString))
                .AddSingleton<IItemRepo<Item>>(new ItemRepo<Item>(db))
                .AddSingleton<IMessageRepo>(new MessageRepo(db))
                .BuildServiceProvider();
            // Setup main business Api
            Api = new Api(Configuration, ServiceProvider);
            // Run initial setup
            var setup = new Setups.Setup(Configuration, ServiceProvider);
            setup.Install();
            MainLoop();
        }
        private static void MainLoop()
        {
            // Wait for user input
            Console.WriteLine("Hit 'q' and enter to abort\n");
            var clientInput = "";
            var tmpInput = "";
            long tickStart = 0;
            long tickPassed = 0;
            long deliverStart = 0;
            long deliverPassed = 0;
            while (clientInput != "q")
            {
                tickPassed = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - tickStart;
                if (tickPassed > 2000)
                {
                    tickStart = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    Tick();
                }
                deliverPassed = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - deliverStart;
                if (deliverPassed > 200)
                {
                    deliverStart = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    DeliverMessages();
                }
                if (Console.KeyAvailable)
                {
                    char key = Console.ReadKey().KeyChar;
                    if (key == 13)
                    {
                        clientInput = tmpInput;
                        tmpInput = "";
                        var itemRepo = ServiceProvider.GetService<IItemRepo<Item>>();
                        var gameItem = itemRepo.ReadUuid(new List<Guid>() { new Guid("487a7282-0cad-4081-be92-83b14671fc23") }).First();
                        var clientMessage = new Message()
                        {
                            Version = ItemVersion.Alpha,
                            Type = MessageType.EventClientInput,
                            From = gameItem,
                            Value = JsonConvert.SerializeObject(new BodyEventClientInput() { Raw = clientInput })
                        };
                        Api.SendMessage(clientMessage, itemRepo.Read());
                    }
                    else
                    {
                        tmpInput += key;
                    }
                }
            }
        }
        /// <summary>
        /// Execute all item scripts that desire a regular periodic event
        /// </summary>
        /// <param name="o"></param>
        private static void Tick()
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
        /// <summary>
        /// Deliver messages to their recipient items and execute each item script
        /// </summary>
        /// <param name="o"></param>
        public static void DeliverMessages()
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
//                messages.ForEach(delegate (Message message)
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
