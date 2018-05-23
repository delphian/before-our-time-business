using BeforeOurTime.Repository.Models.Scripts.Delegates;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Scripts.Delegates.onRawText
{
    /// <summary>
    /// Unformatted raw information provided in a string
    /// </summary>
    public class OnRawTextDelegate : IDelegate
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public OnRawTextDelegate()
        {
        }
        public string GetFunctionName()
        {
            return "onRawText";
        }
        public Type GetArgumentType()
        {
            return typeof(OnRawTextArgument);
        }
        public Type GetReturnType()
        {
            return typeof(OnRawTextReturn);
        }
        public Guid GetId()
        {
            return new Guid("f0a2281e-82df-49d1-91da-40712d70f28f");
        }
    }
}
