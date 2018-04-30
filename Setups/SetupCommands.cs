using System;
using System.Collections.Generic;
using System.Linq;
using BeforeOurTime.Repository.Models;
using BeforeOurTime.Repository.Models.Items;
using Microsoft.Extensions.Configuration;

namespace BeforeOurTime.Business.Setups
{
    public class SetupCommands: ISetup
    {
        public List<Item> Items(
            IConfigurationRoot configuration, 
            IServiceProvider serviceProvider) 
        {
            var itemRepo = (IItemRepo<Item>) serviceProvider.GetService(typeof(IItemRepo<Item>));
            var gameItem = itemRepo.ReadUuid(new List<Guid>() { new Guid("487a7282-0cad-4081-be92-83b14671fc23") }).First();

            var items = new List<Item>();
            items.Add(new Item() {
                Type = ItemType.Command,
                Uuid = new Guid("998b0dd6-4a6e-4930-8245-60effa37279a"),
                UuidType = new Guid("ed187c11-1afe-4750-985a-546ec6198f5f"),
                ParentId = gameItem.Id,
                Children = new List<Item>(),
                Data = @"{}",
                Script = @"
                    function onEventClientInput(e) {
                        if (e.raw == 'help') {
                            log('Available commands are : \'help\', \'look\', \'q\'\n');
                        }
                    };
                "
            });
            items.Add(new Item()
            {
                Type = ItemType.Command,
                Uuid = new Guid("9428d020-b5bd-425d-8b07-91e94ac680f5"),
                UuidType = new Guid("8a3ba31e-df39-4be1-a177-d83daaec8716"),
                ParentId = gameItem.Id,
                Children = new List<Item>(),
                Data = @"{}",
                Script = @"
                    function onEventClientInput(e) {
                        if (e.raw == 'look') {
                            var stats = itemInvoke('fe178ad7-0e33-4111-beaf-6dfcfd548bd5', 'onQuery');
                            log('Nothing to see here...\n' + stats.name);
                        }
                    };
                "
            });
            return items;
        }
    }
}
