using BeforeOurTime.Repository.Models.Accounts;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Attributes;
using BeforeOurTime.Repository.Models.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Terminals
{
    public interface ITerminalManager
    {
        /// <summary>
        /// Create a new terminal
        /// </summary>
        /// <returns></returns>
        Terminal RequestTerminal();
        /// <summary>
        /// Authenticate a terminal to use an account
        /// </summary>
        /// <param name="terminal">Central manager of all client connections regardless of protocol (telnet, websocket, etc)</param>
        /// <param name="name">User name</param>
        /// <param name="password">User password</param>
        /// <returns></returns>
        Account AuthenticateTerminal(Terminal terminal, string name, string password);
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
        /// <summary>
        /// Create a new account and local authentication credentials
        /// </summary>
        /// <param name="terminal">Single generic connection used by the environment to communicate with clients</param>
        /// <param name="name">Friendly account name</param>
        /// <param name="email">Login email address for account</param>
        /// <param name="password">Password for account</param>
        /// <returns></returns>
        Account CreateAccount(Terminal terminal, string name, string email, string password);
    }
}
