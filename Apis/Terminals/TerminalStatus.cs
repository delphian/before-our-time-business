using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Terminals
{
    /// <summary>
    /// Connection status of terminal
    /// </summary>
    public enum TerminalStatus : int
    {
        /// <summary>
        /// Terminal granted but no authentication has taken place
        /// </summary>
        Guest,
        /// <summary>
        /// Terminal is associated with account, but not attached to environment item
        /// </summary>
        Authenticated,
        /// <summary>
        /// Terminal authenticated and is attached it environment item
        /// </summary>
        Attached,
        /// <summary>
        /// Terminal is attached but has been unresponsive
        /// </summary>
        Idle
    }
}
