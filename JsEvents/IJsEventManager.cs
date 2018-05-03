using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.JsEvents
{
    public interface IJsEventManager
    {
        /// <summary>
        /// Message type mapping to js onEvent function and argument
        /// </summary>
        List<JsEventRegistration> GetJsEventRegistrations();
        /// <summary>
        /// Required Js Event handlers an item of TYPE must implement
        /// </summary>
        /// <returns></returns>
        Dictionary<ItemType, List<JsEventRegistration>> GetRequiredJsHandlers();
        /// <summary>
        /// Search an item for missing event handlers based on item's type
        /// </summary>
        /// <param name="item">Item on which Script property will be searched</param>
        /// <returns></returns>
        List<JsEventRegistration> MissingEvent(Item item);
    }
}
