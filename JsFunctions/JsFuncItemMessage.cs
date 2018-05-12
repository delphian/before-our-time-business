using BeforeOurTime.Business.Apis;
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
            Func<Item, string, string, object, bool> itemMessage = delegate (
                Item from,
                string toId,
                string type,
                object msgBody)
            {
                var callback = Api.GetScriptManager().GetCallbackDefinition(type);
                var itemTo = itemRepo.Read(new List<Guid>() { new Guid(toId) }).FirstOrDefault();
                var message = new Message()
                {
                    Sender = from,
                    Callback = callback,
                    Package = JsonConvert.SerializeObject(msgBody)
                };
                Api.SendMessage(message, new List<Item>() { itemTo });
                return true;
            };
            JsEngine.SetValue("_itemMessage", itemMessage);
            JsEngine.Execute("var itemMessage = function(toGuidId, type, msgBody){ return _itemMessage(me, toGuidId.ToString(), type, msgBody) };");
        }
    }
}
