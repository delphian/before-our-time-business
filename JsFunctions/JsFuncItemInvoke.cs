using BeforeOurTime.Business.Apis;
using BeforeOurTime.Repository.Models.Items;
using Jint;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeforeOurTime.Business.JsFunctions
{    
    public class JsFuncItemInvoke : JsFunc, IJsFunc
    {
        public JsFuncItemInvoke(IConfigurationRoot config, IServiceProvider provider, IApi api, Engine jsEngine)
            : base(config, provider, api, jsEngine) { }
        /// <summary>
        /// Add a javascript function to the engine for scripts to call
        /// </summary>
        public void AddFunctions()
        {
            var itemRepo = ServiceProvider.GetService<IItemRepo<Item>>();
            // Box some repository functionality into safe limited javascript functions
            Func<Item, string, string, object> itemInvoke = delegate (Item me, string uuid, string method)
            {
                var item = itemRepo.Read(new List<Guid>() { new Guid(uuid) }).FirstOrDefault();
                object result = new Engine().Execute(item.Script.Trim()).Invoke(method, "{}");
                return result;
            };
            JsEngine.SetValue("_itemInvoke", itemInvoke);
            JsEngine.Execute("var itemInvoke = function(toGuidId, funcName){ return _itemInvoke(me, toGuidId.ToString(), funcName) };");
        }
    }
}
