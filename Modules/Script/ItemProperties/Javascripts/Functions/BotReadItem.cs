using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Script.ItemProperties.Javascripts;
using Jint;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Modules.Script.ItemProperties.Javascripts.Functions
{
    public class BotReadItem : IJavascriptFunction
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
        public BotReadItem(IModuleManager moduleManager)
        {
            ModuleManager = moduleManager;
            Definition = new JavascriptFunctionDefinition()
            {
                Global = true,
                Prototype = "item botReadItem(object itemId)",
                Description = "Read an item",
                Example = @"var item = botReadItem(""c558c1f9-7d01-45f3-bc35-dcab52b5a37c"");
botEmote(300, ""I just read item "" + item.id);"
            };
            ModuleManager.ModuleManagerReadyEvent += () =>
            {
                ModuleManager.GetManager<IJavascriptItemDataManager>().AddFunctionDefinition(Definition);
                var jsEngine = ModuleManager.GetManager<IJavascriptItemDataManager>().GetJSEngine();
                Func<object, Item> botReadItem = (object itemId) =>
                {
                    var guidItemId = Guid.Parse(itemId.ToString());
                    return ModuleManager.GetManager<IItemManager>().Read(guidItemId);
                };
                jsEngine.SetValue("botReadItem", botReadItem);
            };
        }
    }
}
