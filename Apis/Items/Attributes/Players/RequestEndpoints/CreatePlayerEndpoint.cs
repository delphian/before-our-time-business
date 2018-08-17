using BeforeOurTime.Business.Apis;
using BeforeOurTime.Business.Apis.Items.Attributes.Games;
using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Business.Apis.Items.Attributes.Players;
using BeforeOurTime.Business.Apis.Scripts.Delegates.OnTerminalInput;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Models.Items.Attributes;
using BeforeOurTime.Models.Messages.Events.Created;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Requests.Create;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Messages.Responses.Create;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Models.Items.Attributes.Physicals;
using BeforeOurTime.Business.Apis.Messages.RequestEndpoints;
using BeforeOurTime.Models.Items.Attributes.Characters;
using BeforeOurTime.Models.Items.Attributes.Players;

namespace BeforeOurTime.Business.Items.Attributes.Players.RequestEndpoints
{
    public class CreatePlayerEndpoint : IRequestEndpoint
    {
        public CreatePlayerEndpoint()
        {
        }
        /// <summary>
        /// Register to handle a specific set of IO requests
        /// </summary>
        /// <returns></returns>
        public List<Guid> RegisterForRequests()
        {
            return new List<Guid>()
            {
                CreateAccountCharacterRequest._Id
            };
        }
        /// <summary>
        /// Handle terminal request
        /// </summary>
        /// <param name="api"></param>
        /// <param name="terminal"></param>
        /// <param name="terminalRequest"></param>
        public IResponse HandleRequest(IApi api, Terminal terminal, IRequest request, IResponse response)
        {
            if (request.IsMessageType<CreateAccountCharacterRequest>())
            {
                var createPlayerRequest = request.GetMessageAsType<CreateAccountCharacterRequest>();
                var playerItem = api.GetAttributeManager<IPlayerAttributeManager>().Create(
                    new CharacterAttribute()
                    {
                        Health = new CharacterHealth()
                        {
                            Max = 25,
                            Value = 25
                        }
                    },
                    new PhysicalAttribute()
                    {
                        Name = createPlayerRequest.Name,
                        Description = "A player",
                        Weight = 100
                    },
                    new PlayerAttribute()
                    {
                        Name = createPlayerRequest.Name,
                        AccountId = terminal.AccountId.Value
                    },
                    api.GetAttributeManager<IGameAttributeManager>().GetDefaultLocation());
                var createPlayerResponse = new CreateAccountCharacterResponse()
                {
                    _requestInstanceId = request.GetRequestInstanceId(),
                    _responseSuccess = true,
                    CreatedAccountCharacterEvent = new CreatedAccountCharacterEvent()
                    {
                        ItemId = playerItem.Id,
                        Name = playerItem.Name
                    }
                };
                response = createPlayerResponse;
            }
            return response;
        }
    }
}
