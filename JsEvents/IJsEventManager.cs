using BeforeOurTime.Repository.Models.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.JsEvents
{
    public interface IJsEventManager
    {
        /// <summary>
        /// Message type mapping to js onEvent function and argument
        /// </summary>
        Dictionary<MessageType, JsEventHandler> GetMessageToJsEventMapping();
    }
}
