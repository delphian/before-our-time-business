using BeforeOurTime.Business.JsMessageBody;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Repository.Models.Messages.Events.Maps
{
    /// <summary>
    /// Javascript onEvent function name mapping to message type
    /// </summary>
    public static class MapMessageHandlers
    {
        /// <summary>
        /// Javascript onEvent function name mapping to message type
        /// </summary>
        public static Dictionary<MessageType, MessageHandlerAndBody> GetEventJsMapping()
        {
            var jsEvents = new Dictionary<MessageType, MessageHandlerAndBody>()
            {
                { MessageType.EventTick, new MessageHandlerAndBody("onEventTick", typeof(BodyEventTick)) },
                { MessageType.EventItemMoves, new MessageHandlerAndBody("onEventItemMoves", typeof(BodyEventItemMove)) }
            };
            return jsEvents;
        }
    }
}
