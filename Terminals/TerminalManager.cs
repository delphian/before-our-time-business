using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Repository.Models.Accounts;
using BeforeOurTime.Repository.Models.Accounts.Authentication;

namespace BeforeOurTime.Business.Terminals
{
    /// <summary>
    /// Central manager of all client connections regardless of protocol (telnet, websocket, etc)
    /// </summary>
    public class TerminalManager : ITerminalManager
    {
        /// <summary>
        /// Account repository
        /// </summary>
        protected IAccountRepo AccountRepo { set; get; }
        /// <summary>
        /// List of all active terminals
        /// </summary>
        protected List<Terminal> Terminals = new List<Terminal>();
        /// <summary>
        /// Callback definition of function subscribed to OnTerminalCreated
        /// </summary>
        /// <param name="terminal">A single remote connection</param>
        public delegate void TerminalCreated(Terminal terminal);
        /// <summary>
        /// Subscribe to notification after a new terminal has been created
        /// </summary>
        public event TerminalCreated OnTerminalCreated;
        /// <summary>
        /// Callback definition of function subscribed to OnTerminalDestroyed 
        /// </summary>
        /// <param name="terminal">A single remote connection</param>
        public delegate void TerminalDestroyed(Terminal terminal);
        /// <summary>
        /// Subscribe to notification before a terminal is destroyed
        /// </summary>
        public event TerminalDestroyed OnTerminalDestroyed;
        /// <summary>
        /// Constructor
        /// </summary>
        public TerminalManager(IAccountRepo accountRepo)
        {
            AccountRepo = accountRepo;
        }
        /// <summary>
        /// Request the creation of a terminal by first authenticating a username and password
        /// </summary>
        /// <param name="name">User name</param>
        /// <param name="password">User password</param>
        /// <returns></returns>
        public Terminal RequestTerminal(string name, string password)
        {
            var account = AccountRepo
                .Read(
                    new AuthenticationRequest() { PrincipalName = name, PrincipalPassword = password })
                .FirstOrDefault();
            if (account != null)
            {
                // Atach to game object (temporary)
                return CreateTerminal(account.Id, new Guid("487a7282-0cad-4081-be92-83b14671fc23"));
            }
            return null;
        }
        /// <summary>
        /// Create a new terminal and notify subscribers
        /// </summary>
        /// <param name="accountId">Account holder in operation of terminal</param>
        /// <param name="itemUuid">Item currently attached to as terminal's avatar (in system representation)</param>
        /// <returns></returns>
        public Terminal CreateTerminal(int accountId, Guid itemUuid)
        {
            var terminal = new Terminal(accountId, itemUuid);
            Terminals.Add(terminal);
            if (OnTerminalCreated != null)
            {
                OnTerminalCreated(terminal);
            }
            return terminal;
        }
        /// <summary>
        /// Destroy a terminal and notify subscribers
        /// </summary>
        /// <param name="terminal">A single remote connection</param>
        public void DestroyTerminal(Terminal terminal)
        {
            Terminals.Remove(terminal);
            OnTerminalDestroyed((Terminal) terminal.Clone());
            terminal = null;
        }
        /// <summary>
        /// Send a message to a specific terminal
        /// </summary>
        /// <param name="terminalId">Unique terminal identifier</param>
        /// <param name="message">Raw message</param>
        public void SendToTerminalId(Guid terminalId, string message)
        {
            Terminals.FirstOrDefault(x => x.Id == terminalId).SendToTerminal(message);
        }
        /// <summary>
        /// Get list of all active terminals
        /// </summary>
        /// <returns></returns>
        public List<Terminal> GetTerminals()
        {
            return Terminals;
        }
    }
}
