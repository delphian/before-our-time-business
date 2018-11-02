using BeforeOurTime.Models.Messages.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Models.Modules.World.Managers;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Modules.World.Messages.Location.DeleteLocation;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Core.Messages.ItemCrud.DeleteItem;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models.Modules.Terminal.Models;

namespace BeforeOurTime.Business.Modules.World.Managers
{
    public partial class LocationItemManager
    {
        /// <summary>
        /// Create location
        /// </summary>
        /// <param name="message"></param>
        /// <param name="origin">Item that initiated request</param>
        /// <param name="mm">Module manager</param>
        /// <param name="response"></param>
        public IResponse HandleDeleteLocationRequest(
            IMessage message,
            Item origin,
            IModuleManager mm,
            IResponse response)
        {
            var request = message.GetMessageAsType<WorldDeleteLocationRequest>();
            response = HandleRequestWrapper<WorldDeleteLocationResponse>(request, res =>
            {
                var itemManager = mm.GetManager<IItemManager>();
                var location = itemManager.Read(request.LocationItemId);
                var exits = mm.GetManager<IExitItemManager>()
                    .GetLocationExits(location.Id);
                itemManager.Delete(exits, true);
                location = itemManager.Read(request.LocationItemId);
                itemManager.Delete(new List<Item>() { location });
                var deletedItems = exits ?? new List<Item>();
                deletedItems.Add(location);
                var deleteItemEvent = new CoreDeleteItemCrudEvent()
                {
                    Items = deletedItems
                };
                ((WorldDeleteLocationResponse)response).DeleteItemEvent = deleteItemEvent;
                ((WorldDeleteLocationResponse)response)._responseSuccess = true;
                res.SetSuccess(true);
            });
            return response;
        }
    }
}
