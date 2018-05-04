﻿using System;
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
            Api = serviceProvider.GetService<IApi>();
            // Register terminal middleware
            var interfaceType = typeof(ITerminalMiddleware);
            TerminalMiddlewares = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x => (ITerminalMiddleware) Activator.CreateInstance(x, Api))
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
        /// <param name="terminal">Central manager of all client connections regardless of protocol (telnet, websocket, etc)</param>
        /// <param name="name">User name</param>
        /// <param name="password">User password</param>
        /// <returns></returns>
        public Account AuthenticateTerminal(Terminal terminal, string name, string password)
        {
            var account = AccountRepo
                .Read(
                    new AuthenticationRequest() { PrincipalName = name, PrincipalPassword = password })
                .FirstOrDefault();
            return account;
        }
        /// <summary>
        /// Attach a terminal to an environment item as it's avatar
        /// </summary>
        /// <param name="terminal">Central manager of all client connections regardless of protocol (telnet, websocket, etc)</param>
        /// <param name="itemId">Unique item identifier to use as terminal's avatar</param>
        /// <returns></returns>
        public Character AttachTerminal(Terminal terminal, Guid itemId)
        {
            Character avatar = null;
            var character = ItemRepo.Read<Character>(new List<Guid>() { itemId }).FirstOrDefault();
            if (character != null && terminal.AccountId == character.AccountId)
            {
                avatar = character;
            }
            return avatar;
        }
        /// <summary>
        /// Destroy a terminal and notify subscribers
        /// </summary>
        /// <param name="terminal">A single remote connection</param>
        public TerminalManager DestroyTerminal(Terminal terminal)
        {
            Terminals.Remove(terminal);
            OnTerminalDestroyed((Terminal) terminal.Clone());
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
    }
}
