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
    public class BotMoveItem : IJavascriptFunction
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
        public BotMoveItem(IModuleManager moduleManager)
        {
            ModuleManager = moduleManager;
            Definition = new JavascriptFunctionDefinition()
            {
                Global = true,
                Prototype = "void botMoveItem(object itemId, object toId, object originId)",
                Description = "Move this item to new location",
                Example = @"botMoveItem(me.Id, ""07f91a80-a9d3-4d97-b696-40ce0e95df91"", me.Id);"
            };
            ModuleManager.ModuleManagerReadyEvent += () =>
            {
                ModuleManager.GetManager<IJavascriptItemDataManager>().AddFunctionDefinition(Definition);
                var jsEngine = ModuleManager.GetManager<IJavascriptItemDataManager>().GetJSEngine();
                Action<object, object, object> botMoveItem = (itemId, toId, originId) =>
                {
                    var itemManager = ModuleManager.GetManager<IItemManager>();
                    var moveItem = itemManager.Read(Guid.Parse(itemId.ToString()));
                    var toItem = itemManager.Read(Guid.Parse(toId.ToString()));
                    var originItem = (originId != null) ? itemManager.Read(Guid.Parse(originId.ToString())) : null;
                    itemManager.Move(moveItem, toItem, originItem);
                };
                jsEngine.SetValue("botMoveItem", botMoveItem);
            };
        }
    }
}
