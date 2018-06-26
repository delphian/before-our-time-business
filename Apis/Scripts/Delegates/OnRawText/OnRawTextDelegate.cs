using BeforeOurTime.Models.Scripts.Delegates;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Scripts.Delegates.OnRawText
{
    /// <summary>
    /// Unformatted raw information provided in a string
    /// </summary>
    public class OnRawTextDelegate : ScriptDelegate, IDelegate
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public OnRawTextDelegate()
        {
            DelegateId = new Guid("f0a2281e-82df-49d1-91da-40712d70f28f");
            DelegateFunctionName = "onRawText";
            DelegateArgumentType = typeof(OnRawTextArgument);
            DelegateReturnType = typeof(OnRawTextReturn);
        }
    }
}
