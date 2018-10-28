﻿using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Messages.ItemJson.CreateItemJson;
using BeforeOurTime.Models.Modules.Core.Models.Items;
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
        private IResponse HandleCoreCreateItemJsonRequest(
            IMessage message,
            IModuleManager mm,
            ITerminal terminal,
            IResponse response)
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
