using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Repository.Models.Accounts;
using BeforeOurTime.Repository.Models.Accounts.Authentication;
using Microsoft.Extensions.DependencyInjection;
using BeforeOurTime.Business.Apis;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Attributes;
using BeforeOurTime.Business.Apis.Items.Attributes.Interfaces;
using BeforeOurTime.Repository.Models.Items.Attributes.Repos;
using BeforeOurTime.Business.Apis.Accounts;
using BeforeOurTime.Repository.Models.Messages;

namespace BeforeOurTime.Business.Terminals
{
    /// <summary>
    /// Central manager of all client connections regardless of protocol (telnet, websocket, etc)
    /// </summary>
    public class TerminalManager : ITerminalManager
    {
        private IAccountManager AccountManager { set; get; }
        /// <summary>
        /// List of all active terminals
        /// </summary>
        protected List<Terminal> Terminals = new List<Terminal>();
        /// <summary>
        /// Definition of function that may subscribe to OnTerminalCreated
        /// </summary>
        /// <param name="terminal">Single generic connection used by the environment to communicate with clients</param>
        public delegate void TerminalCreated(Terminal terminal);
        /// <summary>
        /// Subscribe to notification after a new terminal has been created
        /// </summary>
        public event TerminalCreated OnTerminalCreated;
        /// <summary>
        /// Definition of function that may subscribe to OnTerminalDestroyed 
        /// </summary>
        /// <param name="terminal">Single generic connection used by the environment to communicate with clients</param>
        public delegate void TerminalDestroyed(Terminal terminal);
        /// <summary>
        /// Subscribe to notification before a terminal is destroyed
        /// </summary>
        public event TerminalDestroyed OnTerminalDestroyed;
        /// <summary>
        /// Constructor
        /// </summary>
        public TerminalManager(
            IServiceProvider serviceProvider
        )
        {
            var scopedProvider = serviceProvider.CreateScope().ServiceProvider;
            AccountManager = scopedProvider.GetService<IAccountManager>();
        }
        /// <summary>
        /// Create a new terminal
        /// </summary>
        /// <returns></returns>
        public Terminal RequestTerminal()
        {
            var terminal = new Terminal(this);
            terminal.Status = TerminalStatus.Guest;
            Terminals.Add(terminal);
            if (OnTerminalCreated != null)
            {
                OnTerminalCreated(terminal);
            }
            return terminal;
        }
        /// <summary>
        /// Authenticate a terminal to use an account
        /// </summary>
        /// <param name="terminal">Single generic connection used by the environment to communicate with clients</param>
        /// <param name="name">User name</param>
        /// <param name="password">User password</param>
        /// <returns></returns>
        public Account AuthenticateTerminal(Terminal terminal, string name, string password)
        {
            return AccountManager.Authenticate(name, password);
        }
        /// <summary>
        /// Destroy a terminal and notify subscribers
        /// </summary>
        /// <param name="terminal">Single generic connection used by the environment to communicate with clients</param>
        public TerminalManager DestroyTerminal(Terminal terminal)
        {
            Terminals.Remove(terminal);
            OnTerminalDestroyed((Terminal)terminal.Clone());
            terminal = null;
            return this;
        }
        /// <summary>
        /// Send a message to a specific terminal
        /// </summary>
        /// <param name="terminalId">Unique terminal identifier</param>
        /// <param name="environmentUpdate"></param>
        public void SendToTerminalId(Guid terminalId, IMessage environmentUpdate)
        {
            Terminals.FirstOrDefault(x => x.Id == terminalId).SendToClient(environmentUpdate);
        }
        /// <summary>
        /// Get list of all active terminals
        /// </summary>
        /// <returns></returns>
        public List<Terminal> GetTerminals()
        {
            return Terminals;
        }
        /// <summary>
        /// Create a new account and local authentication credentials
        /// </summary>
        /// <param name="terminal">Single generic connection used by the environment to communicate with clients</param>
        /// <param name="name">Friendly account name</param>
        /// <param name="email">Login email address for account</param>
        /// <param name="password">Password for account</param>
        /// <returns></returns>
        public Account CreateAccount(Terminal terminal, string name, string email, string password)
        {
            return AccountManager.Create(name, email, password);
        }
    }
}