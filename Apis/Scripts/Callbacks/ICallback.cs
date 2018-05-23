using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Scripts.Callbacks
{
    /// <summary>
    /// Base interface for all script function callback definitions
    /// </summary>
    public interface ICallback
    {
        /// <summary>
        /// Get script function callback name
        /// </summary>
        /// <returns></returns>
        string GetFunctionName();
        /// <summary>
        /// Get script function callback argument type
        /// </summary>
        /// <returns></returns>
        Type GetArgumentType();
        /// <summary>
        /// Get script function callback return value type
        /// </summary>
        /// <returns></returns>
        Type GetReturnType();
    }
}
