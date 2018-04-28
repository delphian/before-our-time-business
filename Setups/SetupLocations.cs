using System;
using System.Collections.Generic;
using BeforeOurTime.Repository.Models.Items;
using Microsoft.Extensions.Configuration;

namespace BeforeOurTime.Business.Setups
{
    public class SetupLocations : ISetup
    {
        public List<Item> Items(
            IConfigurationRoot configuration, 
            IServiceProvider serviceProvider) 
        {
            var items = new List<Item>();
            items.Add(new Item() {
                Type = ItemType.Generic,
                Uuid = new Guid("fe178ad7-0e33-4111-beaf-6dfcfd548bd5"),
                UuidType = new Guid("f08045bc-86b1-4c03-bb71-3b7f95dbd7c2"),
                ParentId = null,
                Children = new List<Item>(),
                Data = @"{
                    sample: 23
                }",
                Script = @"
                    (function() {
                        log(data.sample);
                        data.sample = data.sample + 1;
                    })();
                "
            });
            return items;
        }
    }
}
