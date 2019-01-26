using BeforeOurTime.Models.Modules;
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
    public class BotLog : IJavascriptFunction
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
        public BotLog(IModuleManager moduleManager)
        {
            ModuleManager = moduleManager;
            Definition = new JavascriptFunctionDefinition()
            {
                Global = true,
                Prototype = "void botLog(string message, int? level = null)",
                Description = "Write a message to the server log file",
                Example = @"botLog(""Error executing javascript"");"
            };
        }
        public JavascriptFunctionDefinition GetDefinition()
        {
            return Definition;
        }
        public void CreateFunction(Engine jsEngine)
        {
            Action<object, int?> botLog = (message, level) =>
            {
                level = level ?? (int?)LogLevel.Information;
                ModuleManager.GetLogger().Log((LogLevel)level, message.ToString());
            };
            jsEngine.SetValue("botLog", botLog);
        }
    }
}
