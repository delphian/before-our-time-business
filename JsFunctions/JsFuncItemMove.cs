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
    public class JsFuncItemMove : JsFunc, IJsFunc
    {
        public JsFuncItemMove(IConfigurationRoot config, IServiceProvider provider, IApi api, Engine jsEngine)
            : base(config, provider, api, jsEngine) { }
        /// <summary>
        /// Add a javascript function to the engine for scripts to call
        /// </summary>
        public void AddFunctions()
        {
            var itemRepo = ServiceProvider.GetService<IItemRepo<Item>>();
            // Box some repository functionality into safe limited javascript functions
            Func<string, string, bool> itemMove = delegate (string uuid, string toUuid)
            {
                var item = itemRepo.Read(new List<Guid>() { new Guid(uuid) }).FirstOrDefault();
                var toItem = itemRepo.Read(new List<Guid>() { new Guid(toUuid) }).FirstOrDefault();
                Api.MoveItem(null, toItem, item);
                return true;
            };
            JsEngine.SetValue("itemMove", itemMove);
        }
    }
}
