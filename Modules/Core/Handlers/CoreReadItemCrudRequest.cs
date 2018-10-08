using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules.Core.Messages.ItemCrud.ReadItem;
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
                var readItems = new List<Item>();
                // Read enumerated list of items
                if (request.ItemIds != null)
                {
                    readItems = api.GetItemManager().Read(request.ItemIds);
                }
                ((CoreReadItemCrudResponse)res).CoreReadItemCrudEvent = new CoreReadItemCrudEvent()
                {
                    Items = readItems
                };
                res.SetSuccess(true);
            });
            return response;
        }
    }
}
