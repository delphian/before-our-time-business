using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.JsEvents
{
    /// <summary>
    /// Content of event message. Passed as argument to javascript event listener function
    /// </summary>
    public interface IJsHandler
    {
        /// <summary>
        /// Register javascript event handler
        /// </summary>
        /// <returns></returns>
        JsEventRegistration Register();
    }
}
