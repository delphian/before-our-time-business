using BeforeOurTime.Repository.Models.Scripts.Callbacks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Scripts.Callbacks.onRawText
{
    /// <summary>
    /// Unformatted raw information provided in a string
    /// </summary>
    /// <remarks>
    /// This object will be recieved as the argument to the onRawText script
    /// function callback. Generally the text is simply dumped to the client's
    /// default text output, or discarded entirely.
    /// </remarks>
    public class OnRawTextArgument : ICallbackArgument
    {
        [JsonProperty(PropertyName = "returnType", Order = 10)]
        public string Text { set; get; }
    }
}
