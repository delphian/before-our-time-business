using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules.Core.Messages.ItemCrud.UpdateItem;
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
        private IResponse HandleCoreUpdateItemCrudRequest(IMessage message, IApi api, ITerminal terminal, IResponse response)
        {
            var request = message.GetMessageAsType<CoreUpdateItemCrudRequest>();
            response = HandleRequestWrapper<CoreUpdateItemCrudResponse>(request, res =>
            {
                var updateItemRequest = request.GetMessageAsType<CoreUpdateItemCrudRequest>();
                api.GetItemManager().Update(updateItemRequest.Items);
                var updateItemEvent = new CoreUpdateItemCrudEvent()
                {
                    Items = updateItemRequest.Items
                };
                ((CoreUpdateItemCrudResponse)res).CoreUpdateItemCrudEvent = updateItemEvent;
                res.SetSuccess(true);
            });
            return response;
        }
    }
}
