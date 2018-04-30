using BeforeOurTime.Repository.Models.Items;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Setups
{
    public class Setup
    {
        public static IConfigurationRoot Configuration { set; get; }
        public static IServiceProvider ServiceProvider { set; get; }

        public Setup(IConfigurationRoot configuration, IServiceProvider serviceProvider)
        {
            Configuration = configuration;
            ServiceProvider = serviceProvider;
        }
        public Setup Install()
        {
            InstallItems();
            return this;
        }
        public Setup InstallItems()
        {
            var itemRepo = (IItemRepo<Item>) ServiceProvider.GetService(typeof(IItemRepo<Item>));
            if (!itemRepo.Read(0, 1).Any())
            {
                var gameItem = itemRepo.Create(new List<Item>() { new Item()
                {
                    Type = ItemType.Game,
                    Uuid = new Guid("487a7282-0cad-4081-be92-83b14671fc23"),
                    UuidType = new Guid("75f55af3-3027-404c-b9f0-7b21ead826b2"),
                    ParentId = null,
                    Children = new List<Item>(),
                    Data = @"{
                        sample: 100
                    }",
                    Script = @"
                        function onEventTick(e) {
                            data.sample = data.sample + 1;
                            if (data.sample == 110) {
                                log('\nWe are at 110!\n');
                            }
                        };
                        function onEventClientInput(e) {
                            log('\nA client did something!\n' + e.raw);
                        };
                    "
                } }).First();
                var interfaceType = typeof(ISetup);
                var setupItems = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(x => x.GetTypes())
                    .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Select(x => Activator.CreateInstance(x))
                    .ToList();
                List<Item> items = setupItems
                    .SelectMany(x => ((ISetup)x).Items(Configuration, ServiceProvider))
                    .ToList();
                itemRepo.Create(items);
            }
            return this;
        }
    }
}
