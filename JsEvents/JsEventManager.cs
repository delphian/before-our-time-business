using BeforeOurTime.Repository.Models.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.JsEvents
{
    /// <summary>
    /// Javascript onEvent function name mapping to message type
    /// </summary>
    public static class JsEventManager
    {
        /// <summary>
        /// Javascript onEvent function name mapping to message type
        /// </summary>
        public static Dictionary<MessageType, JsEventHandler> GetEventJsMapping()
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
