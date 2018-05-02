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
    public class JsFuncLog : JsFunc, IJsFunc
    {
        public JsFuncLog(IConfigurationRoot config, IServiceProvider provider, IApi api, Engine jsEngine)
            : base(config, provider, api, jsEngine) { }
        /// <summary>
        /// Add a javascript function to the engine for scripts to call
        /// </summary>
        public void AddFunctions()
        {
            var itemRepo = ServiceProvider.GetService<IItemRepo<Item>>();
            // Box some repository functionality into safe limited javascript functions
            Func<string, Item> getItem = delegate (string uuid)
            {
                return itemRepo.Read(new List<Guid>() { new Guid(uuid) }).FirstOrDefault();
            };
            JsEngine.SetValue("log", new Action<object>(Console.Write));
        }
    }
}
