using System;
using System.Collections.Generic;
using System.Linq;
using BeforeOurTime.Business.Apis.Scripts.Engines;
using BeforeOurTime.Models.Scripts.Delegates;

namespace BeforeOurTime.Business.Apis.Scripts
{
    /// <summary>
    /// Manager to ensure scripts conform and execute to standards
    /// </summary>
    public class ScriptManager : IScriptManager
    {
        protected IScriptEngine ScriptEngine { set; get; }
        /// <summary>
        /// List of available delegate definitions a script may declare
        /// </summary>
        private List<IDelegate> DelegateDefinitions { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scriptEngine"></param>
        public ScriptManager(
            IScriptEngine scriptEngine)
        {
            ScriptEngine = scriptEngine;
            DelegateDefinitions = BuildDelegateDefinitions();
        }
        /// <summary>
        /// Get all functions declared in script
        /// </summary>
        /// <param name="script">Script that provides custom properties and their management</param>
        /// <returns></returns>
        public List<string> GetScriptFunctionDeclarations(string script)
        {
            return ScriptEngine.GetFunctionDeclarations(script.Trim());
        }
        /// <summary>
        /// Get all delegates declared in script and properly implemented
        /// </summary>
        /// <param name="script">Script that provides custom properties and their management</param>
        /// <returns>List of valid delegate declarations</returns>
        public List<IDelegate> GetScriptValidDelegates(string script)
        {
            var scriptValidDelegates = new List<IDelegate>();
            var scriptFunctionDeclarations = GetScriptFunctionDeclarations(script);
            scriptFunctionDeclarations.ForEach(delegate (string functionName)
            {
                if (DelegateDefinitions.Any(x => x.GetFunctionName() == functionName))
                {
                    scriptValidDelegates.Add(DelegateDefinitions.Where(x => x.GetFunctionName() == functionName).First());
                }
            });
            return scriptValidDelegates;
        }
        /// <summary>
        /// Get all delegates declared in script but improperly implemented
        /// </summary>
        /// <param name="script">Script that provides custom properties and their management</param>
        /// <returns>List of invalid delegate declarations, or empty list if script is valid</returns>
        public List<IDelegate> GetScriptInvalidDelegates(string script)
        {
            var scriptInvalidDelegates = new List<IDelegate>();
            return scriptInvalidDelegates;
        }
        /// <summary>
        /// Get script delegate definition based on name
        /// </summary>
        /// <param name="name">Script delegate definition function name</param>
        /// <returns></returns>
        public IDelegate GetDelegateDefinition(string name)
        {
            return DelegateDefinitions.Where(x => x.GetFunctionName() == name).FirstOrDefault();
        }
        /// <summary>
        /// Get script delegate definition based on unique identifier
        /// </summary>
        /// <param name="uuid">Script delegate definition unique identiifer</param>
        /// <returns></returns>
        public IDelegate GetDelegateDefinition(Guid uuid)
        {
            return DelegateDefinitions.Where(x => x.GetId() == uuid).FirstOrDefault();
        }
        /// <summary>
        /// Build list of available delegate definitions
        /// </summary>
        /// <remarks>
        /// Delegate definitions are created by classes that implement IScriptDelegate
        /// </remarks>
        /// <returns></returns>
        private List<IDelegate> BuildDelegateDefinitions()
        {
            var scriptDelegates = new List<IDelegate>();
            var interfaceType = typeof(IDelegate);
            scriptDelegates = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x => (IDelegate) Activator.CreateInstance(x))
                .ToList();
            return scriptDelegates;
        }
    }
}
