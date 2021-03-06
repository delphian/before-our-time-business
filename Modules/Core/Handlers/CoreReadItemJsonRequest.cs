﻿using BeforeOurTime.Models.Json;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models.Modules.Core.Messages.ItemJson;
using BeforeOurTime.Models.Modules.Core.Messages.ItemJson.ReadItemJson;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Terminal.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private IResponse HandleCoreReadItemJsonRequest(
            IMessage message,
            Item origin,
            IModuleManager mm,
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
                            IncludeChildren = request.IncludeChildren,
                            JSON = JsonConvert.SerializeObject(
                                item,
                                Formatting.Indented,
                                new JsonSerializerSettings {
                                    ContractResolver = (request.IncludeChildren == true) ?
                                        new BackupItemContractResolver() :
                                        new DefaultContractResolver()
                                })
                        });
                    });
                    res.SetSuccess(items.Count > 0);
                    res.SetMessage((items.Count == 0) ? "Item not found" : null);
                }
                ((CoreReadItemJsonResponse)res).CoreReadItemJsonEvent = new CoreReadItemJsonEvent()
                {
                    Origin = origin,
                    ItemsJson = coreItemsJson,
                    Items = mm.GetManager<IItemManager>().Read(coreItemsJson.Select(x => Guid.Parse(x.Id)).ToList())
                };
                // Send message to location
                mm.GetManager<IMessageManager>().SendMessageToSiblings(
                    new List<IMessage>() { ((CoreReadItemJsonResponse)res).CoreReadItemJsonEvent },
                    origin,
                    origin);
                res.SetSuccess(true);
            });
            return response;
        }
    }
}
