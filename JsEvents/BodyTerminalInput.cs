using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Repository.Models.Items;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.JsEvents
{
    /// <summary>
    /// Content of event message. Passed as argument to javascript event listener function
    /// </summary>
    public class BodyTerminalInput : Body, IBody
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
