using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Json;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Requests.Create;
using BeforeOurTime.Models.Messages.Requests.List;
using BeforeOurTime.Models.Messages.Requests.Login;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Messages.Responses.Create;
using BeforeOurTime.Models.Messages.Responses.List;
using BeforeOurTime.Models.Messages.Responses.Login;
using BeforeOurTime.Repository.Models.Accounts;
using BeforeOurTime.Repository.Models.Items.Attributes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.Terminals
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
        private ILogger Logger { set; get; }
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
        public Guid? AccountId { set; get; }
        /// <summary>
        /// Unique identifier of character item currently attached to terminal
        /// </summary>
        [JsonProperty(PropertyName = "playerId")]
        public Guid? PlayerId { set; get; }
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
        public delegate IResponse messageToEnvironment(Terminal terminal, IRequest terminalRequest);
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
        public Terminal(
            TerminalManager terminalManager, 
            ILogger logger)
        {
            Id = Guid.NewGuid();
            TerminalManager = terminalManager;
            Logger = logger;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Guest()
        {
            AccountId = null;
            PlayerId = null;
            Status = TerminalStatus.Guest;
            Logger.LogInformation($"Terminal ({Id}) reduced to {Status} status");
        }
        /// <summary>
        /// Authenticate to use an account
        /// </summary>
        /// <param name="name">User name</param>
        /// <returns></returns>
        public void Authenticate(Guid accountId)
        {
            AccountId = accountId;
            PlayerId = null;
            Status = TerminalStatus.Authenticated;
            Logger.LogInformation($"Terminal ({Id}) granted {Status} status as account {AccountId}");
        }
        /// <summary>
        /// Attach to environment item as avatar
        /// </summary>
        /// <param name="itemId">Unique item identifier to use as terminal's avatar</param>
        /// <returns></returns>
        public void Attach(Guid itemId)
        {
            PlayerId = itemId;
            Status = TerminalStatus.Attached;
            Logger.LogInformation($"Terminal ({Id}) granted {Status} status as character {itemId}");
        }
        /// <summary>
        /// Get available characters for terminal attachment
        /// </summary>
        /// <returns>Detached items</returns>
        public List<Item> GetAttachable()
        {
            var accountCharacters = new List<Item>();
            var request = new ListAccountCharactersRequest()
            {
                AccountId = AccountId.Value
            };
            var response = SendToApi(request);
            if (response.IsSuccess())
            {
                var listAccountCharactersResponse = response.GetMessageAsType<ListAccountCharactersResponse>();
                accountCharacters = listAccountCharactersResponse.AccountCharacters;
            }
            return accountCharacters;
        }
        /// <summary>
        /// Create a new character and attach
        /// </summary>
        /// <param name="name">Friendly name of character</param>
        /// <returns></returns>
        public bool CreatePlayer(string name)
        {
            var createPlayerRequest = new CreateAccountCharacterRequest()
            {
                Name = name
            };
            var response = SendToApi(createPlayerRequest);
            if (response.IsSuccess())
            {
                var createPlayerResponse = response.GetMessageAsType<CreateAccountCharacterResponse>();
                PlayerId = createPlayerResponse.CreatedAccountCharacterEvent.ItemId;
                Status = TerminalStatus.Attached;
            }
            return (response.IsSuccess());
        }
        /// <summary>
        /// Send a message to the client
        /// </summary>
        /// <param name="message"></param>
        public void SendToClient(IMessage message)
        {
            OnMessageToTerminal?.Invoke(this, message);
        }
        /// <summary>
        /// Send a message to the environment
        /// </summary>
        /// <param name="request"></param>
        public IResponse SendToApi(IRequest request)
        {
            IResponse response = new Response()
            {
                ResponseSuccess = false
            };
            if (Status == TerminalStatus.Guest)
            {
                if (request.IsMessageType<CreateAccountRequest>() ||
                    request.IsMessageType<LoginRequest>())
                {
                    response = OnMessageToServer?.Invoke(this, request);
                    if (response.IsSuccess())
                    {
                        Guid? accountId;
                        accountId = response.GetMessageAsType<LoginResponse>()?.AccountId;
                        accountId = (accountId == null) ? response.GetMessageAsType<CreateAccountResponse>()?.CreatedAccountEvent?.AccountId : accountId;
                        Authenticate(accountId.Value);
                    }
                }
            }
            else
            {
                if (request.IsMessageType<LoginAccountCharacterRequest>())
                {
                    var playCharacterRequest = request.GetMessageAsType<LoginAccountCharacterRequest>();
                    Attach(playCharacterRequest.ItemId);
                    response = new LoginAccountCharacterResponse() { ResponseSuccess = true };
                }
                else if (request.IsMessageType<LogoutRequest>())
                {
                    response = OnMessageToServer?.Invoke(this, request);
                    if (response.IsSuccess())
                    {
                        Guest();
                    }
                }
                else
                {
                    response = OnMessageToServer?.Invoke(this, request);
                }
            }
            return response;
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
