using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules.Core.Messages.ItemCrud.DeleteItem;
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
        private IResponse HandleCoreDeleteItemCrudRequest(IMessage message, IApi api, ITerminal terminal, IResponse response)
        {
            var request = message.GetMessageAsType<CoreDeleteItemCrudRequest>();
            response = HandleRequestWrapper<CoreDeleteItemCrudResponse>(request, res =>
            {
                var items = api.GetItemManager().Read(request.ItemIds);
                api.GetItemManager().Delete(items, true);
                var deleteItemEvent = new CoreDeleteItemCrudEvent()
                {
                    Items = items
                };
                ((CoreDeleteItemCrudResponse)res).CoreDeleteItemCrudEvent = deleteItemEvent;
                res.SetSuccess(true);
            });
            return response;
        }
    }
}
