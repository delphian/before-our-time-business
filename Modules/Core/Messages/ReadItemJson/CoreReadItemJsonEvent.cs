using BeforeOurTime.Business.Modules.Core.Messages.ReadItemJson;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Json;
using BeforeOurTime.Models.Messages.Events;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Modules.Core.Messages
{
    /// <summary>
    /// Item JSON has been read
    /// </summary>
    public class CoreReadItemJsonEvent : Event, IEvent
    {
        /// <summary>
        /// Unique message identifier
        /// </summary>
        [JsonIgnore]
        [JsonConverter(typeof(GuidJsonConverter))]
        public static Guid _Id = new Guid("bb79f77d-6ed3-43f9-8f44-36cf8d65393b");
        /// <summary>
        /// List of item JSON that has been read
        /// </summary>
        [JsonProperty(PropertyName = "itemsJson", Order = 1100)]
        public List<CoreItemJson> ItemsJson { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public CoreReadItemJsonEvent()
        {
            this.MessageId = _Id;
            this.MessageName = "Item JSON Has Been Read";
        }
    }
}
