using BeforeOurTime.Business.Apis.Items;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules.Core;
using BeforeOurTime.Models.Modules.Core.Messages.ItemGraph;
using BeforeOurTime.Models.Modules.Core.Messages.ItemJson.UpdateItemJson;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Core.Models.Properties;
using BeforeOurTime.Models.Terminals;
using Newtonsoft.Json;
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
        private IResponse HandleCoreUpdateItemJsonRequest(IMessage message, IApi api, ITerminal terminal, IResponse response)
        {
            var request = message.GetMessageAsType<CoreUpdateItemJsonRequest>();
            response = HandleRequestWrapper<CoreUpdateItemJsonResponse>(request, res =>
            {
                request.ItemsJson.ForEach(itemJson =>
                {
                    var item = JsonConvert.DeserializeObject<Item>(itemJson.JSON);
                    ModuleManager.GetItemRepo().Update(item);
                });
                ((CoreUpdateItemJsonResponse)res).CoreUpdateItemJsonEvent = new CoreUpdateItemJsonEvent()
                {
                    ItemsJson = request.ItemsJson
                };
                res.SetSuccess(true);
            });
            return response;
        }
    }
}
