using BeforeOurTime.Models.Messages.Events.Arrivals;
using BeforeOurTime.Models.Scripts.Delegates;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Scripts.Delegates.OnTick
{
    /// <summary>
    /// An item has arrived at same location
    /// </summary>
    public class OnArrivalDelegate : ScriptDelegate, IDelegate
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public OnArrivalDelegate()
        {
            DelegateId = new Guid("e763f28d-da8e-4a30-8860-ba86b013ab08");
            DelegateFunctionName = "onArrival";
            DelegateArgumentType = typeof(ArrivalEvent);
            DelegateReturnType = typeof(OnArrivalReturn);
        }
    }
}
