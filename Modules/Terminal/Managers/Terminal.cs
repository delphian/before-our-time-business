﻿using BeforeOurTime.Models.Exceptions;
using BeforeOurTime.Models.Json;
using BeforeOurTime.Models.Logs;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Account.Managers;
using BeforeOurTime.Models.Modules.Account.Messages.CreateAccount;
using BeforeOurTime.Models.Modules.Account.Messages.CreateCharacter;
using BeforeOurTime.Models.Modules.Account.Messages.LoginAccount;
using BeforeOurTime.Models.Modules.Account.Messages.LoginCharacter;
using BeforeOurTime.Models.Modules.Account.Messages.LogoutAccount;
using BeforeOurTime.Models.Modules.Account.Messages.ReadCharacter;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Terminal.Managers;
using BeforeOurTime.Models.Modules.Terminal.Models;
using BeforeOurTime.Models.Modules.World.ItemProperties.Characters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeforeOurTime.Business.Modules.Terminal.Managers
{
    /// <summary>
    /// Single generic connection used by the environment to communicate with clients
    /// </summary>
    public class Terminal : ITerminal
    {
        protected IModuleManager ModuleManager { set; get; }
        /// <summary>
        /// Central manager of all client connections regardless of protocol (telnet, websocket, etc)
        /// </summary>
        protected ITerminalManager TerminalManager { set; get; }
        private IBotLogger Logger { set; get; }
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
        public delegate IResponse messageToEnvironment(ITerminal terminal, IRequest terminalRequest);
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
            IModuleManager moduleManager)
        {
            Id = Guid.NewGuid();
            ModuleManager = moduleManager;
            TerminalManager = ModuleManager.GetManager<ITerminalManager>();
            Logger = moduleManager.GetLogger();
        }
        /// <summary>
        /// Get unique terminal identifier
        /// </summary>
        /// <returns></returns>
        public Guid GetId()
        {
            return Id;
        }
        /// <summary>
        /// Get current status of terminal
        /// </summary>
        /// <returns></returns>
        public TerminalStatus GetStatus()
        {
            return Status;
        }
        /// <summary>
        /// Set terminal status
        /// </summary>
        /// <returns></returns>
        public ITerminal SetStatus(TerminalStatus status)
        {
            Status = status;
            return this;
        }
        /// <summary>
        /// Get unique player identifier
        /// </summary>
        /// <returns></returns>
        public Guid? GetPlayerId()
        {
            return PlayerId;
        }
        /// <summary>
        /// Get unique account identifier of player
        /// </summary>
        /// <returns></returns>
        public Guid? GetAccountId()
        {
            return AccountId;
        }
        /// <summary>
        /// Get the terminal data bag
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetDataBag()
        {
            return DataBag;
        }
        /// <summary>
        /// Subscribe to messages sent to terminals
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ITerminal SubscribeMessageToTerminal(messageToTerminal callback)
        {
            OnMessageToTerminal += callback;
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Guest()
        {
            AccountId = null;
            PlayerId = null;
            Status = TerminalStatus.Guest;
            Logger.LogInformation($"!! {Id} reduced to {Status} status");
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
            Logger.LogInformation($"!! {Id} granted {Status} status as account {AccountId}");
        }
        /// <summary>
        /// Attach to environment item as avatar
        /// </summary>
        /// <param name="itemId">Unique item identifier to use as terminal's avatar</param>
        /// <returns></returns>
        public void Attach(Guid itemId)
        {
            var characters = ModuleManager.GetManager<IAccountCharacterManager>().ReadByAccount(AccountId.Value);
            if (!characters.Any(x => x.CharacterItemId == itemId))
            {
                throw new BotAuthorizationDeniedException("Character does not belong to account");
            }
            PlayerId = itemId;
            Status = TerminalStatus.Attached;
            Logger.LogInformation($"!! {Id} granted {Status} status as character {itemId}");
        }
        /// <summary>
        /// Get available characters for terminal attachment
        /// </summary>
        /// <returns>Detached items</returns>
        public List<Item> GetAttachable()
        {
            var accountCharacters = new List<Item>();
            var request = new AccountReadCharacterRequest()
            {
                AccountId = AccountId.Value
            };
            var response = SendToApi(request);
            if (response.IsSuccess())
            {
                var listAccountCharactersResponse = response.GetMessageAsType<AccountReadCharacterResponse>();
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
            var createPlayerRequest = new AccountCreateCharacterRequest()
            {
                Name = name
            };
            var response = SendToApi(createPlayerRequest);
            if (response.IsSuccess())
            {
                var createPlayerResponse = response.GetMessageAsType<AccountCreateCharacterResponse>();
                PlayerId = createPlayerResponse.CreatedAccountCharacterEvent.ItemId;
                Status = TerminalStatus.Attached;
            }
            return (response.IsSuccess());
        }
        /// <summary>
        /// Send a message to the client
        /// </summary>
        /// <param name="message"></param>
        public async Task SendToClientAsync(IMessage message)
        {
            var messageJson = JsonConvert.SerializeObject(message);
            Logger.LogInformation($">> {Id} {message.GetMessageName()}");
            Logger.LogDebug($">> {Id} {messageJson}");
            OnMessageToTerminal?.Invoke(this, message);
        }
        /// <summary>
        /// Send a message to the environment
        /// </summary>
        /// <param name="request"></param>
        public IResponse SendToApi(IRequest request)
        {
            var messageJson = JsonConvert.SerializeObject(request);
            Logger.LogInformation($"<< {Id} {request.GetMessageName()}");
            Logger.LogDebug($"<< {Id} {messageJson}");
            IResponse response = new Response()
            {
                _requestInstanceId = request.GetRequestInstanceId(),
                _responseSuccess = false
            };
            if (Status == TerminalStatus.Guest)
            {
                if (request.IsMessageType<AccountCreateAccountRequest>() ||
                    request.IsMessageType<AccountLoginAccountRequest>())
                {
                    response = OnMessageToServer?.Invoke(this, request);
                    if (response.IsSuccess())
                    {
                        Guid? accountId;
                        accountId = response.GetMessageAsType<AccountLoginAccountResponse>()?.Account?.Id;
                        accountId = (accountId == null) ? response.GetMessageAsType<AccountCreateAccountResponse>()?.CreatedAccountEvent?.Account.Id : accountId;
                        Authenticate(accountId.Value);
                    }
                }
            }
            else
            {
                if (request.IsMessageType<AccountLoginCharacterRequest>())
                {
                    var playCharacterRequest = request.GetMessageAsType<AccountLoginCharacterRequest>();
                    response = new AccountLoginCharacterResponse()
                    {
                        _requestInstanceId = request.GetRequestInstanceId(),
                        _responseSuccess = true
                    };
                    try
                    {
                        Attach(playCharacterRequest.ItemId);
                    }
                    catch (Exception e)
                    {
                        response.SetSuccess(false).SetMessage("Can't attach to character. " + e.Message);
                    }
                }
                else if (request.IsMessageType<AccountLogoutAccountRequest>())
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
        public ITerminal Clone()
        {
            return (ITerminal)this.MemberwiseClone();
        }
    }
}
