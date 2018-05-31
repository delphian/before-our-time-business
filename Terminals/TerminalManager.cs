using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Repository.Models.Accounts;
using BeforeOurTime.Repository.Models.Accounts.Authentication;
using Microsoft.Extensions.DependencyInjection;
using BeforeOurTime.Business.Terminals.Middleware;
using BeforeOurTime.Business.Apis;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Attributes;
using BeforeOurTime.Business.Apis.Items.Attributes.Interfaces;
using BeforeOurTime.Repository.Models.Items.Attributes.Repos;
using BeforeOurTime.Business.Apis.IO.Updates.Models;
using BeforeOurTime.Business.Apis.Accounts;

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
        /// Item repository
        /// </summary>
        protected IItemRepo ItemRepo { set; get; }
        /// <summary>
        /// Central data repository for all character items
        /// </summary>
        protected IAttributePlayerRepo DetailCharacterRepo { set; get; }
        private IAccountManager AccountManager { set; get; }
        private IAttributeGameManager DetailGameManager { set; get; }
        private IAttributePlayerManager DetailCharacterManager { set; get; }
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
        /// Features that may insert themselves between terminal and api or terminal and server
        /// </summary>
        public List<ITerminalMiddleware> TerminalMiddlewares = new List<ITerminalMiddleware>();
        /// <summary>
        /// Constructor
        /// </summary>
        public TerminalManager(
            IServiceProvider serviceProvider
        )
        {
            var scopedProvider = serviceProvider.CreateScope().ServiceProvider;
            AccountRepo = scopedProvider.GetService<IAccountRepo>();
            ItemRepo = scopedProvider.GetService<IItemRepo>();
            DetailCharacterRepo = scopedProvider.GetService<IAttributePlayerRepo>();
            AccountManager = scopedProvider.GetService<IAccountManager>();
            DetailGameManager = scopedProvider.GetService<IAttributeGameManager>();
            DetailCharacterManager = scopedProvider.GetService<IAttributePlayerManager>();
            var api = serviceProvider.GetService<IApi>();
            // Register terminal middleware
            var interfaceType = typeof(ITerminalMiddleware);
            TerminalMiddlewares = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x => (ITerminalMiddleware)Activator.CreateInstance(x, api))
                .ToList();
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
        /// Attach a terminal to an environment item as it's avatar
        /// </summary>
        /// <param name="terminal">Single generic connection used by the environment to communicate with clients</param>
        /// <param name="itemId">Unique item identifier to use as terminal's avatar</param>
        /// <returns></returns>
        public AttributePlayer AttachTerminal(Terminal terminal, Guid itemId)
        {
            AttributePlayer avatar = null;
            var character = DetailCharacterRepo.Read(new List<Guid>() { itemId }).FirstOrDefault();
            if (character != null && terminal.AccountId == character.AccountId)
            {
                avatar = character;
            }
            return avatar;
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
        public void SendToTerminalId(Guid terminalId, IIOUpdate environmentUpdate)
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
        /// Get list of all terminal middleware
        /// </summary>
        /// <returns></returns>
        public List<ITerminalMiddleware> GetTerminalMiddleware()
        {
            return TerminalMiddlewares;
        }
        /// <summary>
        /// Get all possible characters that a terminal may attach to
        /// </summary>
        /// <param name="terminal">Single generic connection used by the environment to communicate with clients</param>
        /// <returns></returns>
        public List<AttributePlayer> GetAttachableAvatars(Terminal terminal)
        {
            var avatars = new List<AttributePlayer>();
            if (terminal.AccountId != null)
            {
                avatars = DetailCharacterRepo.ReadCharacters(terminal.AccountId);
            }
            return avatars;
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
        /// <summary>
        /// Create character owned by account owner of terminal
        /// </summary>
        /// <param name="terminal">Single generic connection used by the environment to communicate with clients</param>
        /// <param name="name">Friendly name of character</param>
        /// <returns></returns>
        public AttributePlayer CreateCharacter(Terminal terminal, string name)
        {
            AttributeLocation defaultLocation = DetailGameManager.GetDefaultLocation();
            AttributePlayer character = DetailCharacterManager.Create(
                name,
                terminal.AccountId,
                defaultLocation);
            return character;
        }
    }
}