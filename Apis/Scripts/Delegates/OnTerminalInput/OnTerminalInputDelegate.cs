using BeforeOurTime.Repository.Models.Scripts.Delegates;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Scripts.Delegates.OnTerminalInput
{
    /// <summary>
    /// Structured message from a terminal
    /// </summary>
    public class OnTerminalInputDelegate : IDelegate
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public OnTerminalInputDelegate()
        {
        }
        public string GetFunctionName()
        {
            return "onTerminalInput";
        }
        public Type GetArgumentType()
        {
            return typeof(OnTerminalInputArgument);
        }
        public Type GetReturnType()
        {
            return typeof(OnTerminalInputReturn);
        }
        public Guid GetId()
        {
            return new Guid("052a078e-9814-474b-8c63-b6968b8ebaad");
        }
    }
}
