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
    }
}
