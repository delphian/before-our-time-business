using System;
using System.Collections.Generic;
using System.Linq;
using BeforeOurTime.Repository.Models;
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
            var itemRepo = (IItemRepo<Item>) serviceProvider.GetService(typeof(IItemRepo<Item>));
            var gameItem = itemRepo.ReadUuid(new List<Guid>() { new Guid("487a7282-0cad-4081-be92-83b14671fc23") }).First();

            var items = new List<Item>();
            items.Add(new Item() {
                Type = ItemType.Generic,
                Uuid = new Guid("fe178ad7-0e33-4111-beaf-6dfcfd548bd5"),
                UuidType = new Guid("f08045bc-86b1-4c03-bb71-3b7f95dbd7c2"),
                ParentId = gameItem.Id,
                Children = new List<Item>(),
                Data = @"{
                    sample: 25
                }",
                Script = @"
                    function onEventTick(e) {
                        log(data.sample = data.sample + 1);
                    };
                "
            });
            return items;
        }
    }
}
