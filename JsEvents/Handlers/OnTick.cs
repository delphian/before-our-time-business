using BeforeOurTime.Repository.Models.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.JsEvents
{
    /// <summary>
    /// Content of event message. Passed as argument to javascript event listener function
    /// </summary>
    class OnTick : JsHandler, IJsHandler
    {
        /// <summary>
        /// Register javascript event handler
        /// </summary>
        /// <returns></returns>
        public JsEventRegistration Register()
        {
            return new JsEventRegistration(MessageType.EventTick, "onTick", typeof(OnTick));
        }
    }
}
