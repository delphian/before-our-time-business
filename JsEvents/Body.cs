using BeforeOurTime.Repository.Models.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.JsEvents
{
    /// <summary>
    /// Content of event message. Passed as argument to javascript event listener function
    /// </summary>
    public class Body
    {
        /// <summary>
        /// Message type that invoked this event
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public MessageType Type { set; get; }
    }
}
