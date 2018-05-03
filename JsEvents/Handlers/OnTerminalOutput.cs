using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Repository.Models.Items;
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
    public class OnTerminalOutput : JsHandler, IJsHandler
    {
        /// <summary>
        /// A single remote connection. Source of the message
        /// </summary>
        [JsonProperty(PropertyName = "terminal")]
        public Terminal Terminal { set; get; }
        /// <summary>
        /// Raw unstructured string for terminal consumption
        /// </summary>
        [JsonProperty(PropertyName = "raw")]
        public string Raw { set; get; }
        /// <summary>
        /// Register javascript event handler
        /// </summary>
        /// <returns></returns>
        public JsEventRegistration Register()
        {
            return new JsEventRegistration(MessageType.EventTerminalOutput, "onTerminalOutput", typeof(OnTerminalOutput));
        }
    }
}
