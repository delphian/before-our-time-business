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
    public class OnItemMove : JsHandler, IJsHandler
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
        /// <summary>
        /// Register javascript event handler
        /// </summary>
        /// <returns></returns>
        public JsEventRegistration Register()
        {
            return new JsEventRegistration(MessageType.EventItemMove, "onItemMove", typeof(OnItemMove));
        }
        /// <summary>
        /// Require this JS event handler on any item of TYPE
        /// </summary>
        /// <returns></returns>
        public List<ItemType> RequiredOn()
        {
            return new List<ItemType>()
            {
                 ItemType.Generic,
                 ItemType.Character,
                 ItemType.Npc
            };
        }
    }

}
