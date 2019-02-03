using Microsoft.Extensions.Configuration;
using BeforeOurTime.Business.Modules.Script.ItemProperties.Javascripts.Functions;
using BeforeOurTime.Models.Logs;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Script.ItemProperties.Javascripts;
using Jint;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Models.Exceptions;
using BeforeOurTime.Models.Modules.Core.Managers;

namespace BeforeOurTime.Business.Modules.Script.ItemProperties.Javascripts.Events
{
    /// <summary>
    /// Define an onTick javascript event function
    /// </summary>
    public class JSOnTick : IJavascriptFunction
    {
        /// <summary>
        /// Manage all modules
        /// </summary>
        private IModuleManager ModuleManager { set; get; }
        /// <summary>
        /// Logger
        /// </summary>
        private IBotLogger Logger { set; get; }
        /// <summary>
        /// Item manager
        /// </summary>
        private IItemManager ItemManager { set; get; }
        /// <summary>
        /// Javascript item data manager
        /// </summary>
        private IJavascriptItemDataManager JavascriptItemDataManager { set; get; }
        /// <summary>
        /// Javascript item data repository
        /// </summary>
        private IJavascriptItemDataRepo JavascriptItemDataRepo { set; get; }
        /// <summary>
        /// Tick interval at which scripts will execute
        /// </summary>
        private int TickInterval { set; get; }
        /// <summary>
        /// Current tick interval on it's way to the maximum;
        /// </summary>
        private int TickCount { set; get; } = 0;
        /// <summary>
        /// Description of this function
        /// </summary>
        private JavascriptFunctionDefinition Definition { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="moduleManager"></param>
        public JSOnTick(IModuleManager moduleManager)
        {
            ModuleManager = moduleManager;
            Logger = ModuleManager.GetLogger();
            TickInterval = moduleManager.GetConfiguration()
                .GetSection("Modules")
                .GetSection("Script")
                .GetSection("Managers")
                .GetSection("Javascript")
                .GetValue<int>("TickInterval");
            Definition = new JavascriptFunctionDefinition()
            {
                Global = true,
                Prototype = "onTick [event]",
                Description = "Opportunity to execute code at regular intervals",
                Example = @"var onTick = function() {\n};\n"
            };
            ModuleManager.ModuleManagerReadyEvent += () =>
            {
                ItemManager = ModuleManager.GetManager<IItemManager>();
                JavascriptItemDataManager = ModuleManager.GetManager<IJavascriptItemDataManager>();
                JavascriptItemDataRepo = ModuleManager.GetRepository<IJavascriptItemDataRepo>();
                JavascriptItemDataManager.AddFunctionDefinition(Definition);
                ModuleManager.Ticks += OnTick;
            };
        }
        /// <summary>
        /// Execute onTick javascript function on all items that have it
        /// </summary>
        public void OnTick()
        {
            if (TickCount++ >= TickInterval)
            {
                Logger.LogInformation("Running item scripts");
                var javascriptDatas = JavascriptItemDataRepo.Read();
                javascriptDatas.ForEach(data =>
                {
                    if (data.ScriptFunctions.Contains(":onTick:", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var item = ItemManager.Read(data.DataItemId);
                        if (item == null)
                            throw new BeforeOurTimeException($"No item ({data.DataItemId}) found for javascript data ({data.Id})");
                        JavascriptItemDataManager.ExecuteFunction(item, "onTick");
                    }
                });
                TickCount = 0;
            }
        }
    }
}
