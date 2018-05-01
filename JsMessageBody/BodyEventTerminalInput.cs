using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Repository.Models.Items;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.JsMessageBody
{
    /// <summary>
    /// An event that items may subscribe to receive messages on
    /// </summary>
    public class BodyEventTerminalInput : Body, IBody
    {
        /// <summary>
        /// A single remote connection. Source of the message
        /// </summary>
        [JsonProperty(PropertyName = "terminal")]
        public Terminal Terminal { set; get; }
        /// <summary>
        /// Raw string input from a client
        /// </summary>
        [JsonProperty(PropertyName = "raw")]
        public string Raw { set; get; }
    }
}
