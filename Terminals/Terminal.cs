using BeforeOurTime.Repository.Json;
using BeforeOurTime.Repository.Models.Accounts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Terminals
{
    /// <summary>
    /// A single remote connection
    /// </summary>
    public class Terminal
    {
        /// <summary>
        /// Central manager of all client connections regardless of protocol (telnet, websocket, etc)
        /// </summary>
        protected TerminalManager TerminalManager { set; get; }
        /// <summary>
        /// Unique terminal identifier
        /// </summary>
        /// <remarks>
        /// Automatically generated during instantiation
        /// </remarks>
        [JsonProperty(PropertyName = "id")]
        [JsonConverter(typeof(GuidJsonConverter))]
        public Guid Id { set; get; }
        /// <summary>
        /// Account holder in operation of terminal
        /// </summary>
        [JsonProperty(PropertyName = "accountId")]
        [JsonConverter(typeof(GuidJsonConverter))]
        public Guid AccountId { set; get; }
        /// <summary>
        /// Item currently attached to as terminal's avatar (in system representation)
        /// </summary>
        [JsonProperty(PropertyName = "itemUuid")]
        [JsonConverter(typeof(GuidJsonConverter))]
        public Guid ItemUuid { set; get; }
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
        /// <param name="terminalManager">Central manager of all client connections regardless of protocol (telnet, websocket, etc)</param>
        /// <param name="accountId">Account holder in operation of terminal</param>
        /// <param name="itemUuid">Item currently attached to as terminal's avatar (in system representation)</param>
        public Terminal(TerminalManager terminalManager, Guid accountId, Guid itemUuid)
        {
            TerminalManager = terminalManager;
            Id = Guid.NewGuid();
            AccountId = accountId;
            ItemUuid = itemUuid;
        }
        /// <summary>
        /// Send a message to the terminal
        /// </summary>
        /// <param name="message"></param>
        public void SendToClient(string message)
        {
            var allMiddleware = TerminalManager.GetTerminalMiddleware().Select(x => x).ToList();
            string Next(string msg)
            {
                var middleware = allMiddleware.FirstOrDefault();
                if (middleware != null)
                {
                    allMiddleware.Remove(middleware);
                    return middleware.ToClient(msg, Next);
                }
                return msg;
            }
            message = Next(message);
            if (OnMessageToTerminal != null)
            {
                OnMessageToTerminal(Id, message);
            }
        }
        /// <summary>
        /// Send a message to the server
        /// </summary>
        /// <param name="message"></param>
        public void SendToApi(string message)
        {
            var allMiddleware = TerminalManager.GetTerminalMiddleware().Select(x => x).ToList();
            string Next(string msg)
            {
                var middleware = allMiddleware.FirstOrDefault();
                if (middleware != null)
                {
                    allMiddleware.Remove(middleware);
                    return middleware.ToApi(msg, Next);
                }
                return msg;
            }
            message = Next(message);
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
