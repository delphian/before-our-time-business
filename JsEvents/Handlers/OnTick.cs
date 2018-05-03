using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.JsEvents
{
    /// <summary>
    /// Defines a JS event handler. Javascript function to be called when a message of TYPE is received
    /// </summary>
    class OnTick : JsHandler, IJsHandler
    {
        /// <summary>
        /// Register javascript event handler
        /// </summary>
        /// <returns></returns>
        public JsEventRegistration Register()
        {
            return new JsEventRegistration(MessageType.EventTick, "onTick", typeof(OnTick));
        }
        /// <summary>
        /// Require this JS event handler on any item of TYPE
        /// </summary>
        /// <returns></returns>
        public List<ItemType> RequiredOn()
        {
            return new List<ItemType>()
            {
                 ItemType.Game,
                 ItemType.Npc
            };
        }
    }
}
