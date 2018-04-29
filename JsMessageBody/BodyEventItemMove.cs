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
    public class BodyEventItemMove : Body, IBody
    {
        /// <summary>
        /// Where the item has departd from
        /// </summary>
        [JsonProperty(PropertyName = "from")]
        public Item From { set; get; }
        /// <summary>
        /// Where the item has arrived to
        /// </summary>
        [JsonProperty(PropertyName = "to")]
        public Item To { set; get; }
        /// <summary>
        /// The item itself that is being moved
        /// </summary>
        [JsonProperty(PropertyName = "item")]
        public Item Item { set; get; }
    }
}
