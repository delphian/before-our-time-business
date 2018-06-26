using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Models.Scripts.Delegates;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Scripts.Delegates.OnTerminalInput
{
    /// <summary>
    /// Structured message from a terminal
    /// </summary>
    /// <remarks>
    /// This object will be recieved as the argument to the onTerminalInput script delegate
    /// </remarks>
    public class OnTerminalInputArgument : IDelegateArgument
    {
        /// <summary>
        /// Single generic connection used by the environment to communicate with clients
        /// </summary>
        public Terminal Terminal { set; get; }
        /// <summary>
        /// Raw unstructured message
        /// </summary>
        public string Raw { set; get; }
    }
}
