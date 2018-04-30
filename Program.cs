using BeforeOurTime.Business.Apis;
using BeforeOurTime.Business.JsFunctions;
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
            // Run initial setup
            var setup = new Setups.Setup(Configuration, ServiceProvider);
            setup.Install();
            // Create a Timer object that knows to call our TimerCallback
            // method once every 2000 milliseconds.
            var api = new Api(Configuration, ServiceProvider);
            Timer timerTick = new Timer(Tick, api, 0, 10000);
            Timer timerDeliverMessages = new Timer(DeliverMessages, null, 0, 500);
            // Wait for user input
            Console.WriteLine("Waiting - Hit enter to abort");
            Console.ReadLine();
        }
        /// <summary>
        /// Execute all item scripts that desire a regular periodic event
        /// </summary>
        /// <param name="o"></param>
        public static void Tick(Object o)
        {
            Console.Write("+");
            lock (thisLock)
            {
                var api = (Api)o;
                // Display the date/time when this method got called.
                var itemRepo = ServiceProvider.GetService<IItemRepo<Item>>();
                var gameItem = itemRepo.ReadUuid(new List<Guid>() { new Guid("487a7282-0cad-4081-be92-83b14671fc23") }).First();
                var tickMessage = new Message()
                {
                    Version = ItemVersion.Alpha,
                    Type = MessageType.EventTick,
                    From = gameItem,
                    Value = "{}"
                };
                api.SendMessage(tickMessage, itemRepo.Read());
            }
        }
        /// <summary>
        /// Deliver messages to their recipient items and execute each item script
        /// </summary>
        /// <param name="o"></param>
        public static void DeliverMessages(Object o)
        {
            Console.Write("-");
            var logger = ServiceProvider.GetService<ILogger>();
            lock(thisLock)
            {
                var itemRepo = ServiceProvider.GetService<IItemRepo<Item>>();
                var messageRepo = ServiceProvider.GetService<IMessageRepo>();
                var parser = new Jint.Parser.JavaScriptParser();
                var jsEngine = new Engine();
                // Javascript onEvent function name mapping to message type
                var jsEvents = MapMessageHandlers.GetEventJsMapping();
                // Create script global functions
                GetJsFunctions(Configuration, ServiceProvider, jsEngine);
                // Get messages
                List<Message> messages = messageRepo.Read();
                messageRepo.Delete();
                // Deliver message to each recipient
                messages.ForEach(delegate (Message message)
                {
                    try
                    {
                        var jsProgram = parser.Parse(message.To.Script.Trim());
                        if (jsProgram.FunctionDeclarations.Any(x => x.Id.Name == jsEvents[message.Type].Function))
                        {
                            Type t = jsEvents[message.Type].Type;
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
                    catch (Exception e)
                    {
                        logger.LogError("script failed: " + message.To.Uuid + " " + e.Message);
                    }
                });
            }
        }
        /// <summary>
        /// Create all javascript functions for scripts to use
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="jsEngine"></param>
        public static void GetJsFunctions(IConfigurationRoot configuration, IServiceProvider serviceProvider, Engine jsEngine)
        {
            var interfaceType = typeof(IJsFunc);
            var jsFuncClasses = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x => Activator.CreateInstance(x))
                .ToList();
            jsFuncClasses
                .ForEach(x => ((IJsFunc)x).AddFunctions(configuration, serviceProvider, jsEngine));
        }
    }
}
