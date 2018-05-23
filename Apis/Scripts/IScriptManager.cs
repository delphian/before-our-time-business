using BeforeOurTime.Business.Apis.Scripts.Callbacks;
using BeforeOurTime.Repository.Models.Scripts.Callbacks;
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
        /// Get all callback function names declared by script
        /// </summary>
        /// <param name="script">Javascript that provides custom properties and their management</param>
        /// <returns></returns>
        List<string> GetScriptCallbackDeclarations(string script);
        /// <summary>
        /// Get all properly formated callbacks declared by script
        /// </summary>
        /// <param name="script">Javascript that provides custom properties and their management</param>
        /// <returns>Javascript that provides custom properties and their management</returns>
        List<ICallback> GetScriptCallbackDefinitions(string script);
        /// <summary>
        /// Get all script callback functions declared by script but improperly implemented
        /// </summary>
        /// <param name="script">Javascript that provides custom properties and their management</param>
        /// <returns>List of invalid callback function declarations, or empty list of script is valid</returns>
        List<ICallback> GetScriptInvalidCallbackDeclarations(string script);
        /// <summary>
        /// Get script callback function definition based on name
        /// </summary>
        /// <param name="name">Name of the script callback definition</param>
        /// <returns></returns>
        ICallback GetCallbackDefinition(string name);
    }
}
