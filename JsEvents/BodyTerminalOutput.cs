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
    public class BodyTerminalOutput : Body, IBody
    {
        /// <summary>
        /// Raw unstructured string for terminal consumption
        /// </summary>
        [JsonProperty(PropertyName = "raw")]
        public string Raw { set; get; }
    }
}
