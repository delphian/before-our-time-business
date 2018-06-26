using BeforeOurTime.Models.Scripts.Delegates;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Scripts
{
    /// <summary>
    /// Manager to ensure scripts conform and execute to standards
    /// </summary>
    public interface IScriptManager
    {
        /// <summary>
        /// Get all functions declared in script
        /// </summary>
        /// <param name="script">Script that provides custom properties and their management</param>
        /// <returns></returns>
        List<string> GetScriptFunctionDeclarations(string script);
        /// <summary>
        /// Get all delegates declared in script and properly implemented
        /// </summary>
        /// <param name="script">Script that provides custom properties and their management</param>
        /// <returns>List of valid delegate declarations</returns>
        List<IDelegate> GetScriptValidDelegates(string script);
        /// <summary>
        /// Get all delegates declared in script but improperly implemented
        /// </summary>
        /// <param name="script">Script that provides custom properties and their management</param>
        /// <returns>List of invalid delegate declarations, or empty list if script is valid</returns>
        List<IDelegate> GetScriptInvalidDelegates(string script);
        /// <summary>
        /// Get script delegate definition based on name
        /// </summary>
        /// <param name="name">Script delegate definition function name</param>
        /// <returns></returns>
        IDelegate GetDelegateDefinition(string name);
        /// <summary>
        /// Get script delegate definition based on unique identifier
        /// </summary>
        /// <param name="uuid">Script delegate definition unique identiifer</param>
        /// <returns></returns>
        IDelegate GetDelegateDefinition(Guid uuid);
    }
}
