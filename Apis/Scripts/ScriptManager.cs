using System.Collections.Generic;
using System.Linq;
using BeforeOurTime.Business.Apis.Scripts.Engines;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Scripts.Callbacks;
using Jint.Parser;

namespace BeforeOurTime.Business.Apis.Scripts
{
    /// <summary>
    /// Manager to ensure scripts conform and execute to standards
    /// </summary>
    public class ScriptManager : IScriptManager
    {
        protected IItemRepo<Item> ItemRepo { set; get; }
        protected IScriptCallbackRepo ScriptCallbackRepo { set; get; }
        protected IScriptEngine ScriptEngine { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ItemRepo"></param>
        public ScriptManager(
            IItemRepo<Item> itemRepo,
            IScriptCallbackRepo scriptCallbackRepo,
            IScriptEngine scriptEngine)
        {
            ItemRepo = itemRepo;
            ScriptCallbackRepo = scriptCallbackRepo;
            ScriptEngine = scriptEngine;
        }
        /// <summary>
        /// Get all callback function names declared by script
        /// </summary>
        /// <param name="script">Javascript that provides custom properties and their management</param>
        /// <returns></returns>
        public List<string> GetCallbackDeclarations(string script)
        {
            return ScriptEngine.GetFunctionDeclarations(script.Trim());
        }
        /// <summary>
        /// Get all properly formated callbacks declared by script
        /// </summary>
        /// <param name="script">Javascript that provides custom properties and their management</param>
        /// <returns>Javascript that provides custom properties and their management</returns>
        public List<ScriptCallback> GetCallbacks(string script)
        {
            var scriptCallbacks = new List<ScriptCallback>();
            var callbackNames = GetCallbackDeclarations(script);
            callbackNames.ForEach(delegate (string callbackName)
            {
                var callback = ScriptCallbackRepo.Read(callbackName);
                if (callback != null)
                {
                    scriptCallbacks.Add(callback);
                }
            });
            return scriptCallbacks;
        }
        /// <summary>
        /// Get all script callback functions declared by script but improperly implemented
        /// </summary>
        /// <param name="script">Javascript that provides custom properties and their management</param>
        /// <returns>List of invalid callback function declarations, or empty list of script is valid</returns>
        public List<ScriptCallback> GetInvalidCallbacks(string script)
        {
            var invalidCallbacks = new List<ScriptCallback>();
            return invalidCallbacks;
        }
        /// <summary>
        /// Get script callback function definition based on name
        /// </summary>
        /// <param name="name">Name of the script callback definition</param>
        /// <returns></returns>
        public ScriptCallback GetCallbackDefinition(string name)
        {
            return ScriptCallbackRepo.Read(name);
        }
    }
}
