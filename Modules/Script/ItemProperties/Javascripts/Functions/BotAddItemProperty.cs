using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Core.Models.Properties;
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
    public class BotAddItemProperty : IJavascriptFunction
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
        public BotAddItemProperty(IModuleManager moduleManager)
        {
            ModuleManager = moduleManager;
            Definition = new JavascriptFunctionDefinition()
            {
                Global = true,
                Prototype = "void botAddItemProperty(string propertyClass, object propertyObj)",
                Description = "Dynamically add a property to an item",
                Example = @"var onItemRead = function(item) {
    botAddItemProperty(""BeforeOurTime.Models.Modules.Core.Models.Properties.CommandItemProperty"", {
        ""commands"": [{
            ""itemId"": item.Id,
            ""id"": ""22a73822-6655-4b7b-aa2d-100b5c4a00a7"",
            ""data"": {
                ""like"": 5
            },
            ""name"": ""Run Javascript Callback""
        }]
    });
};"
            };
            ModuleManager.ModuleManagerReadyEvent += () =>
            {
                ModuleManager.GetManager<IJavascriptItemDataManager>().AddFunctionDefinition(Definition);
                var jsEngine = ModuleManager.GetManager<IJavascriptItemDataManager>().GetJSEngine();
                Func<string, object, bool> botAddItemProperty = (string typeName, object propertyObj) => 
                {
                    Item item = (Item)jsEngine.GetValue("me").ToObject();
                    Type propertyType = Type.GetType(typeName + ",BeforeOurTime.Models");
                    var itemProperty = (IItemProperty)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(propertyObj), propertyType);
                    item.AddProperty(propertyType, itemProperty);
                    return true;
                };
                jsEngine.SetValue("botAddItemProperty", botAddItemProperty);
            };
        }
    }
}
