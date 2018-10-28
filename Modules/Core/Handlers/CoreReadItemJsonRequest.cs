﻿using BeforeOurTime.Business.Apis.Items;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models.Modules.Core.Messages.ItemJson;
using BeforeOurTime.Models.Modules.Core.Messages.ItemJson.ReadItemJson;
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
        /// <param name="message"></param>
        /// <param name="mm">Module manager</param>
        /// <param name="terminal">Terminal that initiated request</param>
        /// <param name="response"></param>
        private IResponse HandleCoreReadItemJsonRequest(
            IMessage message,
            IModuleManager mm,
            ITerminal terminal,
            IResponse response)
        {
            var request = message.GetMessageAsType<CoreReadItemJsonRequest>();
            response = HandleRequestWrapper<CoreReadItemJsonResponse>(request, res =>
            {
                var coreItemsJson = new List<CoreItemJson>();
                // Read enumerated list of items
                if (request.ItemIds != null)
                {
                    var items = mm.GetManager<IItemManager>().Read(request.ItemIds);
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
                res.SetSuccess(true);
            });
            return response;
        }
    }
}
