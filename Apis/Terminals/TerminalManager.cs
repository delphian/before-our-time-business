using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using BeforeOurTime.Models.Messages;
using System.Net;
using Microsoft.Extensions.Logging;
using BeforeOurTime.Models.Terminals;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Account.Managers;

namespace BeforeOurTime.Business.Apis.Terminals
{
    /// <summary>
    /// Central manager of all client connections regardless of protocol (telnet, websocket, etc)
    /// </summary>
    public class TerminalManager : ITerminalManager
    {
        private ILogger Logger { set; get; }
        private IAccountManager AccountManager { set; get; }
        /// <summary>
        /// List of all active terminals
        /// </summary>
        protected List<ITerminal> Terminals = new List<ITerminal>();
        /// <summary>
        /// Definition of function that may subscribe to OnTerminalCreated
        /// </summary>
        /// <param name="terminal">Single generic connection used by the environment to communicate with clients</param>
        public delegate void TerminalCreated(ITerminal terminal);
        /// <summary>
        /// Subscribe to notification after a new terminal has been created
        /// </summary>
        public event TerminalCreated OnTerminalCreated;
        /// <summary>
        /// Definition of function that may subscribe to OnTerminalDestroyed 
        /// </summary>
        /// <param name="terminal">Single generic connection used by the environment to communicate with clients</param>
        public delegate void TerminalDestroyed(ITerminal terminal);
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
            AccountManager = scopedProvider.GetService<IModuleManager>().GetManager<IAccountManager>();
            Logger = serviceProvider.GetService<ILogger>();
        }
        /// <summary>
        /// Create a new terminal
        /// </summary>
        /// <param name="serverName">Name of server module</param>
        /// <param name="address">IPAddress of connection</param>
        /// <returns></returns>
        public ITerminal RequestTerminal(string serverName, IPEndPoint address)
        {
            var terminal = new Terminal(this, Logger) as ITerminal;
            terminal.SetStatus(TerminalStatus.Guest);
            Terminals.Add(terminal);
            Logger.LogInformation($"Terminal ({terminal.GetId()}) granted {terminal.GetStatus()} status for {address.ToString()} through {serverName}");
            OnTerminalCreated?.Invoke(terminal);
            return terminal;
        }
        /// <summary>
        /// Destroy a terminal and notify subscribers
        /// </summary>
        /// <param name="terminal">Single generic connection used by the environment to communicate with clients</param>
        public ITerminalManager DestroyTerminal(ITerminal terminal)
        {
            Terminals.Remove(terminal);
            Logger.LogInformation($"Terminal ({terminal.GetId()}) removed");
            OnTerminalDestroyed?.Invoke((ITerminal)terminal.Clone());
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
            Terminals.FirstOrDefault(x => x.GetId() == terminalId).SendToClient(environmentUpdate);
        }
        /// <summary>
        /// Get list of all active terminals
        /// </summary>
        /// <returns></returns>
        public List<ITerminal> GetTerminals()
        {
            return Terminals;
        }
    }
}