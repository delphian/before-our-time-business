using BeforeOurTime.Repository.Models.Scripts.Delegates;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Scripts.Delegates.OnTick
{
    /// <summary>
    /// Periodic poke from the server indicating passage of time
    /// </summary>
    public class OnTickDelegate : ScriptDelegate, IDelegate
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public OnTickDelegate()
        {
            DelegateId = new Guid("a20ef1ab-0e3f-40df-aca9-a70e18e51f32");
            DelegateFunctionName = "onTick";
            DelegateArgumentType = typeof(OnTickArgument);
            DelegateReturnType = typeof(OnTickReturn);
        }
    }
}
