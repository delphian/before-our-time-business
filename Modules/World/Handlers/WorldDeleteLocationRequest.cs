using BeforeOurTime.Models.Messages.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Terminals;
using BeforeOurTime.Models.Modules.World.Managers;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Modules.World.Messages.Location.DeleteLocation;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Core.Messages.ItemCrud.DeleteItem;

namespace BeforeOurTime.Business.Modules.World.Managers
{
    public partial class LocationItemManager
    {
        /// <summary>
        /// Create location
        /// </summary>
        /// <param name="api"></param>
        /// <param name="terminal"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        public IResponse HandleDeleteLocationRequest(IMessage message, IApi api, ITerminal terminal, IResponse response)
        {
            var request = message.GetMessageAsType<WorldDeleteLocationRequest>();
            response = HandleRequestWrapper<WorldDeleteLocationResponse>(request, res =>
            {
                var location = api.GetItemManager().Read(request.LocationItemId);
                var exits = api.GetModuleManager().GetManager<IExitItemManager>()
                    .GetLocationExits(location.Id);
                api.GetItemManager().Delete(exits, true);
                location = api.GetItemManager().Read(request.LocationItemId);
                api.GetItemManager().Delete(new List<Item>() { location });
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
