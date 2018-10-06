using BeforeOurTime.Business.Apis.Items;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules.Core;
using BeforeOurTime.Models.Modules.Core.Messages.ItemGraph;
using BeforeOurTime.Models.Modules.Core.Messages.ItemJson;
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
        private IResponse HandleCoreReadItemJsonRequest(IMessage message, IApi api, ITerminal terminal, IResponse response)
        {
            var request = message.GetMessageAsType<CoreReadItemJsonRequest>();
            response = HandleRequestWrapper<CoreReadItemJsonResponse>(request, res =>
            {
                var coreItemsJson = new List<CoreItemJson>();
                // Read enumerated list of items
                if (request.ItemIds != null)
                {
                    var items = api.GetItemManager().Read(request.ItemIds);
                    items.ForEach(item =>
                    {
                        coreItemsJson.Add(new CoreItemJson()
                        {
                            Id = item.Id.ToString(),
                            JSON = JsonConvert.SerializeObject(item, Formatting.Indented)
                        });
                    });
                }
                ((CoreReadItemJsonResponse)res).CoreReadItemJsonEvent = new CoreReadItemJsonEvent()
                {
                    ItemsJson = coreItemsJson
                };
            });
            return response;
        }
    }
}
