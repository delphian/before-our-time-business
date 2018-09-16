using BeforeOurTime.Models.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Modules.Core.Messages.ReadItemJson
{
    /// <summary>
    /// A unique item identifier and it's corrisponding JSON encoding
    /// </summary>
    public class CoreItemJson
    {
        /// <summary>
        /// Unique item identifier
        /// </summary>
        [JsonProperty(PropertyName = "id", Order = 1000)]
        [JsonConverter(typeof(GuidJsonConverter))]
        public Guid Id { set; get; }
        /// <summary>
        /// Item encoded as JSON
        /// </summary>
        [JsonProperty(PropertyName = "json", Order = 1100)]
        public string JSON { set; get; }
    }
}
