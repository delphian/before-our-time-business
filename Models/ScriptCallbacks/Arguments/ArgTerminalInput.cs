using BeforeOurTime.Business.Terminals;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Models.ScriptCallbacks.Arguments
{
    class ArgTerminalInput
    {
        public Terminal Terminal { set; get; }
        public string Raw { set; get; }
    }
}
