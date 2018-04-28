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
            itemRepo.Delete();
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
            return this;
        }
    }
}
