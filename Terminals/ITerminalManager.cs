using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Terminals
{
    public interface ITerminalManager
    {
        /// <summary>
        /// Request the creation of a terminal by first authenticating a username and password
        /// </summary>
        /// <param name="name">User name</param>
        /// <param name="password">User password</param>
        /// <returns></returns>
        Terminal RequestTerminal(string name, string password);
        /// <summary>
        /// Send a message to a specific terminal
        /// </summary>
        /// <param name="terminalId">Unique terminal identifier</param>
        /// <param name="message">Raw message</param>
        void SendToTerminalId(Guid terminalId, string message);
    }
}
