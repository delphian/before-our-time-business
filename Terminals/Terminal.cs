using BeforeOurTime.Repository.Json;
using BeforeOurTime.Repository.Models.Accounts;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Attributes;
using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Messages.Requests;
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
        /// Player attribute currently attached to as terminal's avatar (in system representation)
        /// </summary>
        [JsonProperty(PropertyName = "playerId")]
        public Guid PlayerId { set; get; }
        /// <summary>
        /// General purpose databag at the disposal of the client server
        /// </summary>
        [JsonProperty(PropertyName = "dataBag")]
        public Dictionary<string, string> DataBag = new Dictionary<string, string>();
        /// <summary>
        /// Define delgate that terminal can use to send requests to the environemnt
        /// </summary>
        /// <param name="terminal"></param>
        /// <param name="terminalRequest"></param>
        public delegate void messageToEnvironment(Terminal terminal, IRequest terminalRequest);
        /// <summary>
        /// Define delgate that environment can use to update terminal
        /// </summary>
        /// <param name="terminal"></param>
        /// <param name="terminalUpdate"></param>
        public delegate void messageToTerminal(Terminal terminal, IMessage terminalUpdate);
        /// <summary>
        /// Terminals may attach to this event to receive updates from the environment
        /// </summary>
        public event messageToTerminal OnMessageToTerminal;
        /// <summary>
        /// Environment may attach to this event to receive requests from terminals
        /// </summary>
        public event messageToEnvironment OnMessageToServer;
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
                PlayerId = character.Id;
                Status = TerminalStatus.Attached;
            }
            return (character != null);
        }
        /// <summary>
        /// Get available characters for terminal attachment
        /// </summary>
        /// <returns></returns>
        public List<Item> GetAttachable()
        {
            return TerminalManager.GetAttachableAvatars(this);
        }
        /// <summary>
        /// Create a new account and local authentication credentials. Authenticate on new account
        /// </summary>
        /// <param name="name">Friendly account name</param>
        /// <param name="email">Login email address for account</param>
        /// <param name="password">Password for account</param>
        /// <returns></returns>
        public bool CreateAccount(string name, string email, string password)
        {
            var account = TerminalManager.CreateAccount(this, name, email, password);
            if (account != null)
            {
                AccountId = account.Id;
                Status = TerminalStatus.Authenticated;
            }
            return (account != null);
        }
        /// <summary>
        /// Create a new character and attach
        /// </summary>
        /// <param name="name">Friendly name of character</param>
        /// <returns></returns>
        public bool CreateCharacter(string name)
        {
            var player = TerminalManager.CreateCharacter(this, name);
            if (player != null)
            {
                PlayerId = player.Id;
                Status = TerminalStatus.Attached;
            }
            return (player != null);
        }
        /// <summary>
        /// Send a message to the terminal
        /// </summary>
        /// <param name="environmentUpdate"></param>
        public void SendToClient(IMessage environmentUpdate)
        {
            if (OnMessageToTerminal != null)
            {
                OnMessageToTerminal(this, environmentUpdate);
            }
        }
        /// <summary>
        /// Send a message to the server
        /// </summary>
        /// <param name="terminalRequest"></param>
        public void SendToApi(IRequest terminalRequest)
        {
            if (OnMessageToServer != null)
            {
                OnMessageToServer(this, terminalRequest);
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
