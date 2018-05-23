using BeforeOurTime.Repository.Models.Scripts.Delegates;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Scripts.Delegates.onRawText
{
    /// <summary>
    /// Unformatted raw information provided in a string
    /// </summary>
    /// <remarks>
    /// This object will be recieved as the argument to the onRawText script
    /// delegate. Generally the text is simply dumped to the client's
    /// default text output, or discarded entirely.
    /// </remarks>
    public class OnRawTextArgument : IDelegateArgument
    {
        [JsonProperty(PropertyName = "text", Order = 10)]
        public string Text { set; get; }
    }
}
