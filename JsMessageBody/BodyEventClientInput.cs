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
    public class BodyEventClientInput : Body, IBody
    {
        /// <summary>
        /// Raw string input from a client
        /// </summary>
        [JsonProperty(PropertyName = "raw")]
        public string Raw { set; get; }
    }
}
