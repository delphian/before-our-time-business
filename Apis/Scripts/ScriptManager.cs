using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BeforeOurTime.Business.Apis.Scripts.Callbacks;
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
        protected IItemRepo ItemRepo { set; get; }
        protected IScriptEngine ScriptEngine { set; get; }
        /// <summary>
        /// List of definitions for all script callback functions
        /// </summary>
        private List<ICallback> CallbackDefinitions { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ItemRepo"></param>
        public ScriptManager(
            IItemRepo itemRepo,
            IScriptEngine scriptEngine)
        {
            ItemRepo = itemRepo;
            ScriptEngine = scriptEngine;
            CallbackDefinitions = BuildCallbackDefinitions();
        }
        /// <summary>
        /// Get all callback function names declared by script
        /// </summary>
        /// <param name="script">Javascript that provides custom properties and their management</param>
        /// <returns></returns>
        public List<string> GetScriptCallbackDeclarations(string script)
        {
            return ScriptEngine.GetFunctionDeclarations(script.Trim());
        }
        /// <summary>
        /// Get all properly formated callbacks declared by script
        /// </summary>
        /// <param name="script">Javascript that provides custom properties and their management</param>
        /// <returns>Javascript that provides custom properties and their management</returns>
        public List<ICallback> GetScriptCallbackDefinitions(string script)
        {
            var callbacks = new List<ICallback>();
            var callbackDeclarations = GetScriptCallbackDeclarations(script);
            callbackDeclarations.ForEach(delegate (string functionName)
            {
                if (CallbackDefinitions.Any(x => x.GetFunctionName() == functionName))
                {
                    callbacks.Add(CallbackDefinitions.Where(x => x.GetFunctionName() == functionName).First());
                }
            });
            return callbacks;
        }
        /// <summary>
        /// Get all script callback functions declared by script but improperly implemented
        /// </summary>
        /// <param name="script">Javascript that provides custom properties and their management</param>
        /// <returns>List of invalid callback function declarations, or empty list of script is valid</returns>
        public List<ICallback> GetScriptInvalidCallbackDeclarations(string script)
        {
            var invalidCallbacks = new List<ICallback>();
            return invalidCallbacks;
        }
        /// <summary>
        /// Get script callback function definition based on name
        /// </summary>
        /// <param name="name">Name of the script callback definition</param>
        /// <returns></returns>
        public ICallback GetCallbackDefinition(string name)
        {
            return CallbackDefinitions.Where(x => x.GetFunctionName() == name).FirstOrDefault();
        }
        /// <summary>
        /// Get script callback function definition based on unique identifier
        /// </summary>
        /// <param name="uuid">script function definition unique identiifer</param>
        /// <returns></returns>
        public ICallback GetCallbackDefinition(Guid uuid)
        {
            return CallbackDefinitions.Where(x => x.GetId() == uuid).FirstOrDefault();
        }
        /// <summary>
        /// Build a list of script function callback definitions
        /// </summary>
        /// <returns></returns>
        private List<ICallback> BuildCallbackDefinitions()
        {
            var callbacks = new List<ICallback>();
            var interfaceType = typeof(ICallback);
            callbacks = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x => (ICallback) Activator.CreateInstance(x))
                .ToList();
            return callbacks;
        }
    }
}
