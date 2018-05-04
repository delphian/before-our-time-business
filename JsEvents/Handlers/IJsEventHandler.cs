﻿using BeforeOurTime.Repository.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.JsEvents
{
    /// <summary>
    /// Content of event message. Passed as argument to javascript event listener function
    /// </summary>
    public interface IJsEventHandler
    {
        /// <summary>
        /// Register javascript event handler
        /// </summary>
        /// <returns></returns>
        JsEventRegistration Register();
        /// <summary>
        /// Require this JS event handler on any item of TYPE
        /// </summary>
        /// <returns></returns>
        List<ItemType> RequiredOn();
    }
}