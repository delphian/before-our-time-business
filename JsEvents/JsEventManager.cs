using BeforeOurTime.Repository.Models.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.JsEvents
{
    /// <summary>
    /// Message type mapping to js onEvent function callback and argument type
    /// </summary>
    public class JsEventManager : IJsEventManager
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public JsEventManager()
        {

        }
        /// <summary>
        /// Message type mapping to js onEvent function and argument
        /// </summary>
        public Dictionary<MessageType, JsEventHandler> GetMessageToJsEventMapping()
        {
            var jsEvents = new Dictionary<MessageType, JsEventHandler>()
            {
                { MessageType.EventTick, new JsEventHandler("onTick", typeof(BodyTick)) },
                { MessageType.EventItemMove, new JsEventHandler("onItemMove", typeof(BodyItemMove)) },
                { MessageType.EventTerminalInput, new JsEventHandler("onTerminalInput", typeof(BodyTerminalInput)) }
            };
            return jsEvents;
        }
    }
}
