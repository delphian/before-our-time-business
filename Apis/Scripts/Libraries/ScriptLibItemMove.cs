using BeforeOurTime.Business.Apis;
using BeforeOurTime.Business.Apis.Scripts.Engines;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Repository.Models.Items;
using Jint;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeforeOurTime.Business.Apis.Scripts.Libraries
{    
    public class ScriptLibItemMove : ScriptLib, IScriptLib
    {
        public ScriptLibItemMove(IConfigurationRoot config, IServiceProvider provider, IApi api, IScriptEngine engine)
            : base(config, provider, api, engine) { }
        /// <summary>
        /// Add a javascript function to the engine for scripts to call
        /// </summary>
        public void AddFunctions()
        {
            var itemRepo = ServiceProvider.GetService<IItemRepo>();
            // Box some repository functionality into safe limited javascript functions
            Func<Item, string, string, bool> itemMove = delegate (Item me, string uuid, string toUuid)
            {
                var item = itemRepo.Read(new List<Guid>() { new Guid(uuid) }).FirstOrDefault();
                var toItem = itemRepo.Read(new List<Guid>() { new Guid(toUuid) }).FirstOrDefault();
                Api.GetItemManager().Move(item, toItem, null);
                return true;
            };
            Engine
                .SetValue("_itemMove", itemMove)
                .Execute("var itemMove = function(uuid, toUuid){ return _itemMove(me, uuid.ToString(), toUuid.ToString()) };");
        }
    }
}
