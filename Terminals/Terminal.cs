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
    /// Single generic connection used by the environment to communicate with clients
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
        /// Connection status of terminal
        /// </summary>
        public TerminalStatus Status { set; get; }
        /// <summary>
        /// Account holder in operation of terminal
        /// </summary>
        [JsonProperty(PropertyName = "accountId")]
        [JsonConverter(typeof(GuidJsonConverter))]
        public Guid AccountId { set; get; }
        /// <summary>
        /// Item currently attached to as terminal's avatar (in system representation)
        /// </summary>
        [JsonProperty(PropertyName = "avatarId")]
        [JsonConverter(typeof(GuidJsonConverter))]
        public Guid AvatarId { set; get; }
        /// <summary>
        /// General purpose databag at the disposal of the client server
        /// </summary>
        [JsonProperty(PropertyName = "dataBag")]
        public Dictionary<string, string> DataBag = new Dictionary<string, string>();
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
        public Terminal(TerminalManager terminalManager)
        {
            TerminalManager = terminalManager;
            Id = Guid.NewGuid();
        }
        /// <summary>
        /// Authenticate to use an account
        /// </summary>
        /// <param name="name">User name</param>
        /// <param name="password">User password</param>
        /// <returns></returns>
        public bool Authenticate(string name, string password)
        {
            var account = TerminalManager.AuthenticateTerminal(this, name, password);
            if (account != null)
            {
                AccountId = account.Id;
                Status = TerminalStatus.Authenticated;
            }
            return (account != null);
        }
        /// <summary>
        /// Attach to environment item as avatar
        /// </summary>
        /// <param name="itemId">Unique item identifier to use as terminal's avatar</param>
        /// <returns></returns>
        public bool Attach(Guid itemId)
        {
            var character = TerminalManager.AttachTerminal(this, itemId);
            if (character != null)
            {
                AvatarId = character.Id;
                Status = TerminalStatus.Attached;
            }
            return (character != null);
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
        /// Get the terminal status
        /// </summary>
        /// <returns></returns>
        public TerminalStatus GetTerminalStatus()
        {
            return Status;
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
