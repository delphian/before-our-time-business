using BeforeOurTime.Repository.Models.Scripts.Callbacks;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Scripts.Callbacks.onTick
{
    /// <summary>
    /// Periodic poke from the server indicating passage of time
    /// </summary>
    public class OnTickCallback : ICallback
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public OnTickCallback()
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
