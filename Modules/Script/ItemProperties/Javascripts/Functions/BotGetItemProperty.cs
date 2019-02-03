using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Script.ItemProperties.Javascripts;
using Jint;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Modules.Script.ItemProperties.Javascripts.Functions
{
    public class BotGetItemProperty : IJavascriptFunction
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
        public BotGetItemProperty(IModuleManager moduleManager)
        {
            ModuleManager = moduleManager;
            Definition = new JavascriptFunctionDefinition()
            {
                Global = true,
                Prototype = "object botGetItemProperty(Item item, string partialPropertyName)",
                Description = "Get property of an item. Returns null if no property found.",
                Example = @"var property = botGetItemProperty(me, ""VisibleItemProperty"");
botLog(""Item is named "" + property.name);"
            };
            ModuleManager.ModuleManagerReadyEvent += () =>
            {
                ModuleManager.GetManager<IJavascriptItemDataManager>().AddFunctionDefinition(Definition);
                var jsEngine = ModuleManager.GetManager<IJavascriptItemDataManager>().GetJSEngine();
                Func<Item, string, object> botGetItemProperty = (Item item, string partialPropertyName) =>
                {
                    var key = item.Properties.Keys
                                             .Where(x => x.Name.Contains(partialPropertyName))
                                             .FirstOrDefault();
                    return (item.Properties.ContainsKey(key)) ? item.Properties[key] : null;
                };
                jsEngine.SetValue("botGetItemProperty", botGetItemProperty);
            };
        }
    }
}
