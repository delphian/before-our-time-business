using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Script.ItemProperties.Javascripts;
using Jint;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Modules.Script.ItemProperties.Javascripts.Functions
{
    public class BotListCount : IJavascriptFunction
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
        public BotListCount(IModuleManager moduleManager)
        {
            ModuleManager = moduleManager;
            Definition = new JavascriptFunctionDefinition()
            {
                Global = true,
                Prototype = "int botListCount(IList list)",
                Description = "Count the number of items in a c# list",
                Example = @"var count = BotListCount(item.children);"
            };
            ModuleManager.ModuleManagerReadyEvent += () =>
            {
                ModuleManager.GetManager<IJavascriptItemDataManager>().AddFunctionDefinition(Definition);
                var jsEngine = ModuleManager.GetManager<IJavascriptItemDataManager>().GetJSEngine();
                Func<IList, int> listCount = (IList list) =>
                {
                    return list.Count;
                };
                jsEngine.SetValue("botListCount", listCount);
            };
        }
    }
}
