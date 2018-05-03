﻿using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Repository.Models.Items;
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
    public class OnTerminalOutput : JsEventHandler, IJsEventHandler
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
        /// <summary>
        /// Require this JS event handler on any item of TYPE
        /// </summary>
        /// <returns></returns>
        public List<ItemType> RequiredOn()
        {
            return new List<ItemType>()
            {
                 ItemType.Character
            };
        }
    }
}
