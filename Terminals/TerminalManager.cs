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
        protected IItemRepo<Item> ItemRepo { set; get; }
        /// <summary>
        /// Central data repository for all character items
        /// </summary>
        protected ICharacterRepo CharacterRepo { set; get; }
        /// <summary>
        /// Interface to the core environment
        /// </summary>
        protected IApi Api { set; get; }
        /// <summary>
        /// List of all active terminals
        /// </summary>
        protected List<Terminal> Terminals = new List<Terminal>();
        /// <summary>
        /// Callback definition of function subscribed to OnTerminalCreated
        /// </summary>
        /// <param name="terminal">Single generic connection used by the environment to communicate with clients</param>
        public delegate void TerminalCreated(Terminal terminal);
        /// <summary>
        /// Subscribe to notification after a new terminal has been created
        /// </summary>
        public event TerminalCreated OnTerminalCreated;
        /// <summary>
        /// Callback definition of function subscribed to OnTerminalDestroyed 
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
        public TerminalManager(IServiceProvider serviceProvider)
        {
            var scopedProvider = serviceProvider.CreateScope().ServiceProvider;
            AccountRepo = scopedProvider.GetService<IAccountRepo>();
            ItemRepo = scopedProvider.GetService<IItemRepo<Item>>();
            CharacterRepo = scopedProvider.GetService<ICharacterRepo>();
            Api = serviceProvider.GetService<IApi>();
            // Register terminal middleware
            var interfaceType = typeof(ITerminalMiddleware);
            TerminalMiddlewares = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x => (ITerminalMiddleware)Activator.CreateInstance(x, Api))
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
            return Api.GetAccountManager().Authenticate(name, password);
        }
        /// <summary>
        /// Attach a terminal to an environment item as it's avatar
        /// </summary>
        /// <param name="terminal">Single generic connection used by the environment to communicate with clients</param>
        /// <param name="itemId">Unique item identifier to use as terminal's avatar</param>
        /// <returns></returns>
        public Character AttachTerminal(Terminal terminal, Guid itemId)
        {
            Character avatar = null;
            var character = CharacterRepo.Read(new List<Guid>() { itemId }).FirstOrDefault();
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
        /// <param name="message">Raw message</param>
        public void SendToTerminalId(Guid terminalId, string message)
        {
            Terminals.FirstOrDefault(x => x.Id == terminalId).SendToClient(message);
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
        public List<Character> GetAttachableAvatars(Terminal terminal)
        {
            var avatars = new List<Character>();
            if (terminal.AccountId != null)
            {
                avatars = CharacterRepo.ReadAvatars(terminal.AccountId);
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
            return Api.GetAccountManager().Create(name, email, password);
        }
        /// <summary>
        /// Create character owned by account owner of terminal
        /// </summary>
        /// <param name="terminal">Single generic connection used by the environment to communicate with clients</param>
        /// <param name="name">Friendly name of character</param>
        /// <returns></returns>
        public Character CreateCharacter(Terminal terminal, string name)
        {
            Character character = Api.GetItemManager().CreateCharacter(
                name, 
                terminal.AccountId, 
                new Guid("e74713f3-9ea8-45e5-9715-3b019222af90"));
            return character;
        }
    }
}