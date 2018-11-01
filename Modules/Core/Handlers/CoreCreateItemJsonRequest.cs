using BeforeOurTime.Models.Json;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Messages.ItemJson;
using BeforeOurTime.Models.Modules.Core.Messages.ItemJson.CreateItemJson;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Terminal.Models;
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
        /// <param name="message"></param>
        /// <param name="mm">Module manager</param>
        /// <param name="terminal">Terminal that initiated request</param>
        /// <param name="response"></param>
        private IResponse HandleCoreCreateItemJsonRequest(
            IMessage message,
            IModuleManager mm,
            ITerminal terminal,
            IResponse response)
        {
            var request = message.GetMessageAsType<CoreCreateItemJsonRequest>();
            response = HandleRequestWrapper<CoreCreateItemJsonResponse>(request, res =>
            {
                var items = JsonConvert.DeserializeObject<List<Item>>(request.ItemJson);
                ModuleManager.GetItemRepo().Create(items);
                var ItemsJson = new List<CoreItemJson>();
                items.ForEach(item =>
                {
                    ItemsJson.Add(new CoreItemJson()
                    {
                        Id = item.Id.ToString(),
                        IncludeChildren = true,
                        JSON = JsonConvert.SerializeObject(
                            item,
                            Formatting.Indented,
                            new JsonSerializerSettings
                            {
                                ContractResolver = new BackupItemContractResolver()
                            })
                    });
                });
                ((CoreCreateItemJsonResponse)res).CoreCreateItemJsonEvent = new CoreCreateItemJsonEvent()
                {
                    ItemsJson = ItemsJson
                };
                res.SetSuccess(true);
            });
            return response;
        }
    }
}
