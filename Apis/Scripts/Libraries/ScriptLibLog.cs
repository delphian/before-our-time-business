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
    public class ScriptLibLog : ScriptLib, IScriptLib
    {
        public ScriptLibLog(IConfigurationRoot config, IServiceProvider provider, IApi api, IScriptEngine engine)
            : base(config, provider, api, engine) { }
        /// <summary>
        /// Add a javascript function to the engine for scripts to call
        /// </summary>
        public void AddFunctions()
        {
            var itemRepo = ServiceProvider.GetService<IItemRepo>();
            // Box some repository functionality into safe limited javascript functions
            Func<Item, string, bool> Log = delegate (Item me, string message)
            {
                Console.WriteLine(message);
                return true;
            };
            Engine
                .SetValue("_log", Log)
                .Execute("var log = function(message){ return _log(me, message) };");
        }
    }
}
