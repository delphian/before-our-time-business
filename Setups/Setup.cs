using BeforeOurTime.Repository.Models.Items;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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
            if (itemRepo.ReadUuid(new List<Guid>() { new Guid("fe178ad7-0e33-4111-beaf-6dfcfd548bd5") }).Count == 0)
            {
                itemRepo.Create(new List<Item>()
                {
                    new Item()
                    {
                        Type = ItemType.Generic,
                        Uuid = new Guid("fe178ad7-0e33-4111-beaf-6dfcfd548bd5"),
                        UuidType = new Guid("f08045bc-86b1-4c03-bb71-3b7f95dbd7c2"),
                        ParentId = null,
                        Children = new List<Item>(),
                        Data = @"{
                            count: 23
                        }",
                        Script = @"
                            (function() {
                                bot.log(bot.me.data.count);
                            })();
                        "
                    }
                });
            }
            return this;
        }
    }
}
