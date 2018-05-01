﻿using BeforeOurTime.Repository.Models.Accounts;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Terminals
{
    /// <summary>
    /// A single remote connection
    /// </summary>
    public class Terminal
    {
        /// <summary>
        /// Unique terminal identifier
        /// </summary>
        /// <remarks>
        /// Automatically generated during instantiation
        /// </remarks>
        public Guid Id { set; get; }
        /// <summary>
        /// Account holder in operation of terminal
        /// </summary>
        public Account Account { set; get; }
        /// <summary>
        /// Define delgate that terminal and server can use to exchange messages
        /// </summary>
        /// <param name="terminalId"></param>
        /// <param name="message"></param>
        public delegate void messageOnTerminal(Guid terminalId, string message);
        /// <summary>
        /// Terminals may attach to this event to receive messages from server
        /// </summary>
        public event messageOnTerminal OnMessageToTerminal;
        /// <summary>
        /// Server may attach to this event to receive messages from terminal
        /// </summary>
        public event messageOnTerminal OnMessageToServer;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="account">Account holder in operation of terminal</param>
        public Terminal(Account account)
        {
            Id = Guid.NewGuid();
            Account = account;
        }
        /// <summary>
        /// Send a message to the terminal
        /// </summary>
        /// <param name="message"></param>
        public void SendToTerminal(string message)
        {
            if (OnMessageToTerminal != null)
            {
                OnMessageToTerminal(Id, message);
            }
        }
        /// <summary>
        /// Send a message to the server
        /// </summary>
        /// <param name="message"></param>
        public void SendToServer(string message)
        {
            if (OnMessageToServer != null)
            {
                OnMessageToServer(Id, message);
            }
        }
        /// <summary>
        /// Clone the terminal
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
