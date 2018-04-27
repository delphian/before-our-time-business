using BeforeOurTime.Repository.Dbs.EF;
using BeforeOurTime.Repository.Models;
using BeforeOurTime.Repository.Models.Items;
using BeforeOutTime.Repository.Dbs.EF;
using Jint;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                .BuildServiceProvider();

            var setup = new Setups.Setup(Configuration, ServiceProvider);
            setup.Install();

            var itemRepo = ServiceProvider.GetService<IItemRepo<Item>>();
            var item = itemRepo.ReadUuid(new List<Guid>() { new Guid("fe178ad7-0e33-4111-beaf-6dfcfd548bd5") }).First();

            var engine = new Engine()
                .SetValue("log", new Action<object>(Console.WriteLine))
                .SetValue("me", item)
                .SetValue("data", JObject.Parse(item.Data));
            engine.Execute(item.Script);

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
