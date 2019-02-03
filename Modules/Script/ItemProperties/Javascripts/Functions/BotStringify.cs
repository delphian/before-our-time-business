using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Script.ItemProperties.Javascripts;
using Jint;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Modules.Script.ItemProperties.Javascripts.Functions
{
    public class BotStringify : IJavascriptFunction
    {
        /// <summary>
        /// Manage all modules
        /// </summary>
        private IModuleManager ModuleManager { set; get; }
        /// <summary>
        /// Description of this function
        /// </summary>
        private JavascriptFunctionDefinition Definition { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="moduleManager"></param>
        public BotStringify(IModuleManager moduleManager)
        {
            ModuleManager = moduleManager;
            Definition = new JavascriptFunctionDefinition()
            {
                Global = true,
                Prototype = "string botStringify(object obj)",
                Description = "Stringify a javascript or c# object. Regular JSON.stringify will not work on a c# object.",
                Example = @"var json = botStringify(object obj);"
            };
            ModuleManager.ModuleManagerReadyEvent += () =>
            {
                ModuleManager.GetManager<IJavascriptItemDataManager>().AddFunctionDefinition(Definition);
                var jsEngine = ModuleManager.GetManager<IJavascriptItemDataManager>().GetJSEngine();
                Func<object, string> botStringify = JsonConvert.SerializeObject;
                jsEngine.SetValue("botStringify", botStringify);
            };
        }
    }
}
