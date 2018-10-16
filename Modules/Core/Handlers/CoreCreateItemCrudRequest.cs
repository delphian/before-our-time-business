using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules.Core.Messages.ItemCrud.CreateItem;
using BeforeOurTime.Models.Terminals;
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
        /// <param name="api"></param>
        /// <param name="message"></param>
        /// <param name="terminal"></param>
        /// <param name="response"></param>
        private IResponse HandleCoreCreateItemCrudRequest(IMessage message, IApi api, ITerminal terminal, IResponse response)
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
