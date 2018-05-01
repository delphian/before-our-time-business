﻿using BeforeOurTime.Business.Apis;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Messages;
using Jint;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeforeOurTime.Business.JsFunctions
{    
    public class JsFuncItemMessage : JsFunc, IJsFunc
    {
        public JsFuncItemMessage(IConfigurationRoot config, IServiceProvider provider, IApi api, Engine jsEngine)
            : base(config, provider, api, jsEngine) { }
        /// <summary>
        /// Add a javascript function to the engine for scripts to call
        /// </summary>
        public void AddFunctions()
        {
            var itemRepo = ServiceProvider.GetService<IItemRepo<Item>>();
            // Box some repository functionality into safe limited javascript functions
            Func<string, string, object, bool> itemMessage = delegate (
                string uuidFrom, 
                string uuidTo,
                object messageBody)
            {
                var itemFrom = itemRepo.ReadUuid(new List<Guid>() { new Guid(uuidFrom) }).FirstOrDefault();
                var itemTo = itemRepo.ReadUuid(new List<Guid>() { new Guid(uuidTo) }).FirstOrDefault();
                var message = new Message()
                {
                    From = itemFrom,
                    Version = ItemVersion.Alpha,
                    Type = MessageType.EventTick,
                    Value = JsonConvert.SerializeObject(messageBody)
                };
                Api.SendMessage(message, new List<Item>() { itemTo });
                return true;
            };
            JsEngine.SetValue("itemMessage", itemMessage);
        }
    }
}
