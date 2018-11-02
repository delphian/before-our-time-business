using BeforeOurTime.Models.Json;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Messages.ItemJson.UpdateItemJson;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Terminal.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
        private IResponse HandleCoreUpdateItemJsonRequest(
            IMessage message,
            Item origin,
            IModuleManager mm,
            IResponse response)
        {
            var request = message.GetMessageAsType<CoreUpdateItemJsonRequest>();
            response = HandleRequestWrapper<CoreUpdateItemJsonResponse>(request, res =>
            {
                request.ItemsJson.ForEach(itemJson =>
                {
                    var item = JsonConvert.DeserializeObject<Item>(
                        itemJson.JSON,
                        new JsonSerializerSettings
                        {
                            ContractResolver = (itemJson.IncludeChildren == true) ?
                                new BackupItemContractResolver() :
                                new DefaultContractResolver()
                        });
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
