using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Messages.ItemCrud.CreateItem;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Terminal.Models;
using BeforeOurTime.ModelsModels.Modules.Core.Messages.ItemCrud.CreateItem;
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
        /// <param name="origin">Item that initiated request</param>
        /// <param name="mm">Module manager</param>
        /// <param name="response"></param>
        private IResponse HandleCoreCreateItemCrudRequest(
            IMessage message,
            Item origin,
            IModuleManager mm,
            IResponse response)
        {
            var request = message.GetMessageAsType<CoreCreateItemCrudRequest>();
            response = HandleRequestWrapper<CoreCreateItemCrudResponse>(request, res =>
            {
                res.SetSuccess(false);
            });
            return response;
        }
    }
}
