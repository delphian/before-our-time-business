using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models.Modules.Core.Messages.ItemCrud.DeleteItem;
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
        private IResponse HandleCoreDeleteItemCrudRequest(
            IMessage message,
            IModuleManager mm,
            ITerminal terminal,
            IResponse response)
        {
            var request = message.GetMessageAsType<CoreDeleteItemCrudRequest>();
            response = HandleRequestWrapper<CoreDeleteItemCrudResponse>(request, res =>
            {
                var items = mm.GetManager<IItemManager>().Read(request.ItemIds);
                mm.GetManager<IItemManager>().Delete(items, true);
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
