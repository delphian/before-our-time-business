using BeforeOurTime.Models;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules.Core.Messages.ItemCrud.ReadItem;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Terminals;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Modules.Core
{
    public partial class CoreModule
    {
        /// <summary>
        /// Handle a message
        /// </summary>
        /// <param name="api"></param>
        /// <param name="message"></param>
        /// <param name="terminal"></param>
        /// <param name="response"></param>
        private IResponse HandleCoreReadItemCrudRequest(IMessage message, IApi api, ITerminal terminal, IResponse response)
        {
            var request = message.GetMessageAsType<CoreReadItemCrudRequest>();
            response = HandleRequestWrapper<CoreReadItemCrudResponse>(request, res =>
            {
                var managers = new List<IItemModelManager>();
                var readItems = new List<Item>();
                // Read enumerated list of items
                if (request.ItemIds != null)
                {
                    readItems = api.GetItemManager().Read(request.ItemIds);
                    res.SetSuccess(true);
                }
                if (request.ItemTypes != null)
                {
                    request.ItemTypes.ForEach(itemType =>
                    {
                        var type = Type.GetType(itemType + ",BeforeOurTime.Models");
                        managers.AddRange(api.GetModuleManager().GetManagers(type));
                    });
                    res.SetSuccess(true);
                }
                if (request.ItemDataTypes != null)
                {
                    request.ItemDataTypes.ForEach(dataType =>
                    {
                        var type = Type.GetType(dataType + ",BeforeOurTime.Models");
                        managers.AddRange(api.GetModuleManager().GetManagersOfData(type));
                    });
                    res.SetSuccess(true);
                }
                if (request.ItemPropertyTypes != null)
                {
                    request.ItemPropertyTypes.ForEach(propertyType =>
                    {
                        var type = Type.GetType(propertyType + ",BeforeOurTime.Models");
                        managers.AddRange(api.GetModuleManager().GetManagersOfProperty(type));
                    });
                    res.SetSuccess(true);
                }
                if (managers.Count > 0)
                {
                    managers.ForEach(manager =>
                    {
                        var itemIds = manager.GetItemIds();
                        readItems.AddRange(api.GetItemManager().Read(itemIds));
                    });
                }
                ((CoreReadItemCrudResponse)res).CoreReadItemCrudEvent = new CoreReadItemCrudEvent()
                {
                    Items = readItems
                };
            });
            return response;
        }
    }
}
