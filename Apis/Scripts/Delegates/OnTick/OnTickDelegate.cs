using BeforeOurTime.Repository.Models.Messages.Events;
using BeforeOurTime.Repository.Models.Scripts.Delegates;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Scripts.Delegates.OnTick
{
    /// <summary>
    /// Game tick
    /// </summary>
    public class OnTickDelegate : ScriptDelegate, IDelegate
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public OnTickDelegate()
        {
            DelegateId = new Guid("88d81bff-1a43-497a-81e6-965ef8b37634");
            DelegateFunctionName = "onTick";
            DelegateArgumentType = typeof(TickEvent);
            DelegateReturnType = typeof(OnTickReturn);
        }
    }
}
