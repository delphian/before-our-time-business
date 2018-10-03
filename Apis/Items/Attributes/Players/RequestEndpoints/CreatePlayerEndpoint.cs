﻿using BeforeOurTime.Business.Apis;
using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Business.Apis.Items.Attributes.Players;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Models.ItemAttributes;
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
using BeforeOurTime.Models.ItemAttributes.Physicals;
using BeforeOurTime.Business.Apis.Messages.RequestEndpoints;
using BeforeOurTime.Models.ItemAttributes.Characters;
using BeforeOurTime.Models.ItemAttributes.Players;
using Microsoft.Extensions.Logging;
using BeforeOurTime.Models.ItemAttributes.Visibles;
using BeforeOurTime.Business.Modules.Core;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Terminals;
using BeforeOurTime.Models.Modules.Core;
using BeforeOurTime.Models.Exceptions;

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
        public IResponse HandleRequest(IApi api, ITerminal terminal, IRequest request, IResponse response)
        {
            if (request.IsMessageType<CreateAccountCharacterRequest>())
            {
                response = new CreateAccountCharacterResponse()
                {
                    _responseSuccess = false,
                    _requestInstanceId = request.GetRequestInstanceId()
                };
                try
                {
                    var defaultLocationId = api.GetModuleManager().GetModule<ICoreModule>()?.GetDefaultLocation()?.Id;
                    if (defaultLocationId == null)
                    {
                        throw new BeforeOurTimeException("Default location could not be found");
                    }
                    var createPlayerRequest = request.GetMessageAsType<CreateAccountCharacterRequest>();
                    var playerItem = api.GetAttributeManager<IPlayerAttributeManager>().Create(
                        new VisibleAttribute()
                        {
                            Name = createPlayerRequest.Name,
                            Description = "A bold new player"
                        },
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
                            AccountId = terminal.GetAccountId().Value
                        },
                        defaultLocationId.Value);
                    ((CreateAccountCharacterResponse)response)._responseSuccess = true;
                    ((CreateAccountCharacterResponse)response).CreatedAccountCharacterEvent = new CreatedAccountCharacterEvent()
                    {
                        ItemId = playerItem.Id,
                        Name = playerItem.Visible.Name
                    };
                }
                catch (Exception e)
                {
                    var traverseExceptions = e;
                    while (traverseExceptions != null)
                    {
                        ((CreateAccountCharacterResponse)response)._responseMessage += traverseExceptions.Message + ". ";
                        traverseExceptions = traverseExceptions.InnerException;
                    }
                    api.GetLogger().LogException(((CreateAccountCharacterResponse)response)._responseMessage, e);
                }
            }
            return response;
        }
    }
}
