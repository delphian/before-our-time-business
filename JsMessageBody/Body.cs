using BeforeOurTime.Repository.Models.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.JsMessageBody
{
    /// <summary>
    /// An event that items may subscribe to receive messages on
    /// </summary>
    public class Body
    {
        /// <summary>
        /// Specific event type. Determines the properties of the event itself
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public MessageType Type { set; get; }
    }
}
