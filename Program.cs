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
            Timer t = new Timer(Tick, null, 0, 8000);
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
            // Display the date/time when this method got called.
            Console.Write("+");
            var itemRepo = ServiceProvider.GetService<IItemRepo<Item>>();
            var gameItem = itemRepo.ReadUuid(new List<Guid>() { new Guid("487a7282-0cad-4081-be92-83b14671fc23") }).First();
            var items = itemRepo.Read();
            var messages = new List<Message>();
            items.ForEach(delegate (Item item)
            {
                messages.Add(new Message()
                {
                    Version = ItemVersion.Alpha,
                    Type = MessageType.EventTick,
                    To = item,
                    From = gameItem,
                    Value = "{}"
                });
            });
            DeliverMessages(messages, ServiceProvider);
            // Force a garbage collection to occur for this demo.
            GC.Collect();
        }
        /// <summary>
        /// Deliver messages to their recipient items and execute each item script
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="serviceProvider"></param>
        public static void DeliverMessages(List<Message> messages, IServiceProvider serviceProvider)
        {
            var itemRepo = serviceProvider.GetService<IItemRepo<Item>>();
            // Box some repository functionality into safe limited javascript functions
            Func<string, Item> getItem = delegate (string uuid)
            {
                return itemRepo.ReadUuid(new List<Guid>() { new Guid(uuid) }).FirstOrDefault();
            };
            // Javascript onEvent function name mapping to message type
            var jsEvents = JsMapping.GetEventJsMapping();
            var parser = new Jint.Parser.JavaScriptParser();
            var engine = new Engine()
                .SetValue("log", new Action<object>(Console.Write))
                .SetValue("getItem", getItem);
            // Deliver message to each recipient
            messages.ForEach(delegate (Message message)
            {
                try
                {
                    var jsProgram = parser.Parse(message.To.Script.Trim());
                    if (jsProgram.FunctionDeclarations.Any(x => x.Id.Name == jsEvents[message.Type].Function))
                    {
                        Type t = jsEvents[message.Type].Type;
                        engine
                            .SetValue("me", message.To)
                            .SetValue("_data", JsonConvert.SerializeObject(JsonConvert.DeserializeObject(message.To.Data)))
                            .Execute("var data = JSON.parse(_data);")
                            .Execute(message.To.Script)
                            .Invoke(
                                jsEvents[message.Type].Function,
                                JsonConvert.DeserializeObject(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(message.Value)))
                            );
                        message.To.Data = JsonConvert.SerializeObject(engine.GetValue("data").ToObject());
                        itemRepo.Update(new List<Item>() { message.To });
                    }
                    else
                    {
                        Console.WriteLine("\n" + message.To.Uuid + ": No matching event for: " + jsEvents[message.Type].Function);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("\nscript failed: " + message.To.Uuid + " " + e.Message);
                }
            });
        }
    }
}
