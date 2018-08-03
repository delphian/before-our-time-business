using BeforeOurTime.Models.Messages;
using BeforeOurTime.Repository.Models.Accounts;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace BeforeOurTime.Business.Apis.Terminals
{
    public interface ITerminalManager
    {
        /// <summary>
        /// Create a new terminal
        /// </summary>
        /// <param name="serverName">Name of server module</param>
        /// <param name="address">IPAddress of connection</param>
        /// <returns></returns>
        Terminal RequestTerminal(string serverName, IPEndPoint address);
        /// <summary>
        /// Send a message to a specific terminal
        /// </summary>
        /// <param name="terminalId">Unique terminal identifier</param>
        /// <param name="environmentUpdate"></param>
        void SendToTerminalId(Guid terminalId, IMessage environmentUpdate);
        /// <summary>
        /// Destroy a terminal and notify subscribers
        /// </summary>
        /// <param name="terminal">A single remote connection</param>
        TerminalManager DestroyTerminal(Terminal terminal);
        /// <summary>
        /// Get list of all active terminals
        /// </summary>
        /// <returns></returns>
        List<Terminal> GetTerminals();
    }
}
