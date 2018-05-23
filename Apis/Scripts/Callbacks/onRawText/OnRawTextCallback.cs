﻿using BeforeOurTime.Repository.Models.Scripts.Callbacks;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Scripts.Callbacks.onRawText
{
    /// <summary>
    /// Unformatted raw information provided in a string
    /// </summary>
    public class OnRawTextCallback : ICallback
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public OnRawTextCallback()
        {
        }
        public string GetFunctionName()
        {
            return "onRawText";
        }
        public Type GetArgumentType()
        {
            return typeof(OnRawTextArgument);
        }
        public Type GetReturnType()
        {
            return typeof(OnRawTextReturn);
        }
        public Guid GetId()
        {
            return new Guid("f0a2281e-82df-49d1-91da-40712d70f28f");
        }
    }
}
