﻿using BeforeOurTime.Repository.Models.Scripts.Delegates;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Scripts.Delegates.onTick
{
    /// <summary>
    /// Periodic poke from the server indicating passage of time
    /// </summary>
    public class OnTickDelegate : IDelegate
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public OnTickDelegate()
        {
        }
        public string GetFunctionName()
        {
            return "onTick";
        }
        public Type GetArgumentType()
        {
            return typeof(OnTickArgument);
        }
        public Type GetReturnType()
        {
            return typeof(OnTickReturn);
        }
        public Guid GetId()
        {
            return new Guid("a20ef1ab-0e3f-40df-aca9-a70e18e51f32");
        }
    }
}
