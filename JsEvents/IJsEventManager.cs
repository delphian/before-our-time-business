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
    }
}
