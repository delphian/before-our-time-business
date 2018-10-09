using BeforeOurTime.Business.Apis.Items;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules.Core;
using BeforeOurTime.Models.Modules.Core.Messages.ItemGraph;
using BeforeOurTime.Models.Modules.Core.Messages.ItemJson;
using BeforeOurTime.Models.Modules.Core.Messages.ItemJson.CreateItemJson;
using BeforeOurTime.Models.Modules.Core.Messages.ItemJson.ReadItemJson;
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
        private IResponse HandleCoreCreateItemJsonRequest(IMessage message, IApi api, ITerminal terminal, IResponse response)
        {
            var request = message.GetMessageAsType<CoreCreateItemJsonRequest>();
            response = HandleRequestWrapper<CoreCreateItemJsonResponse>(request, res =>
            {
                request.ItemsJson.ForEach(itemJson =>
                {
                    var item = JsonConvert.DeserializeObject<Item>(itemJson.JSON);
                    ModuleManager.GetItemRepo().Create(item);
                });
                ((CoreCreateItemJsonResponse)res).CoreCreateItemJsonEvent = new CoreCreateItemJsonEvent()
                {
                    ItemsJson = request.ItemsJson
                };
                res.SetSuccess(true);
            });
            return response;
        }
    }
}
