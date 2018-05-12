using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Scripts.Engines
{
    /// <summary>
    /// Parse and execute item scripts
    /// </summary>
    public interface IScriptEngine
    {
        /// <summary>
        /// Get all function declarations in a script
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        List<string> GetFunctionDeclarations(string script);
        /// <summary>
        /// Set a script variable to a value
        /// </summary>
        /// <param name="name">Name of the variable</param>
        /// <param name="value">Value to assign</param>
        /// <returns></returns>
        IScriptEngine SetValue(string name, object value);
        /// <summary>
        /// Get a script variable value
        /// </summary>
        /// <param name="name">Name of the variable</param>
        /// <returns></returns>/
        object GetValue(string name);
        /// <summary>
        /// Execute a script
        /// </summary>
        /// <param name="script">Script code to execute</param>
        /// <returns></returns>
        IScriptEngine Execute(string script);
        /// <summary>
        /// Execute a single function already loaded into the engine
        /// </summary>
        /// <param name="name"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        IScriptEngine Invoke(string name, object argument);
    }
}
