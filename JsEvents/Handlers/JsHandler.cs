using BeforeOurTime.Repository.Models.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.JsEvents
{
    /// <summary>
    /// Defines a JS event handler. Javascript function to be called when a message of TYPE is received
    /// </summary>
    public class JsHandler
    {
        /// <summary>
        /// Message type that invoked this event
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public MessageType Type { set; get; }
    }
}
