using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models.Modules.Core.Messages.ItemCrud.UpdateItem;
using BeforeOurTime.Models.Modules.Terminal.Models;
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
        /// <param name="message"></param>
        /// <param name="mm">Module manager</param>
        /// <param name="terminal">Terminal that initiated request</param>
        /// <param name="response"></param>
        private IResponse HandleCoreUpdateItemCrudRequest(
            IMessage message,
            IModuleManager mm,
            ITerminal terminal,
            IResponse response)
        {
            var request = message.GetMessageAsType<CoreUpdateItemCrudRequest>();
            response = HandleRequestWrapper<CoreUpdateItemCrudResponse>(request, res =>
            {
                var updateItemRequest = request.GetMessageAsType<CoreUpdateItemCrudRequest>();
                mm.GetManager<IItemManager>().Update(updateItemRequest.Items);
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
