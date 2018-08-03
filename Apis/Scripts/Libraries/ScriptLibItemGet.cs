using BeforeOurTime.Business.Apis;
using BeforeOurTime.Business.Apis.Scripts.Engines;
using BeforeOurTime.Models.Items;
using Jint;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeforeOurTime.Business.Apis.Scripts.Libraries
{    
    public class ScriptLibItemGet : ScriptLib, IScriptLib
    {
        public ScriptLibItemGet(IConfigurationRoot config, IServiceProvider provider, IApi api, IScriptEngine engine)
            : base(config, provider, api, engine) { }
        /// <summary>
        /// Add a javascript function to the engine for scripts to call
        /// </summary>
        public void AddFunctions()
        {
            var itemRepo = ServiceProvider.GetService<IItemRepo>();
            // Box some repository functionality into safe limited javascript functions
            Func<Item, string, Item> itemGet = delegate (Item me, string uuid)
            {
                return itemRepo.Read(new List<Guid>() { new Guid(uuid) }).FirstOrDefault();
            };
            Engine
                .SetValue("_itemGet", itemGet)
                .Execute("var itemGet = function(toGuidId){ return _itemGet(me, toGuidId.ToString()) };");
        }
    }
}
