using BeforeOurTime.Business.Apis.Items.Attributes.Interfaces;
using BeforeOurTime.Business.Apis.Scripts.Delegates.OnTerminalInput;
using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Attributes;
using BeforeOurTime.Repository.Models.Items.Attributes.Exits;
using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Messages.Events;
using BeforeOurTime.Repository.Models.Messages.Requests;
using BeforeOurTime.Repository.Models.Messages.Requests.Look;
using BeforeOurTime.Repository.Models.Messages.Responses;
using BeforeOurTime.Repository.Models.Messages.Responses.Create;
using BeforeOurTime.Repository.Models.Messages.Responses.Enumerate;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO.Requests.Handlers
{
    public class IOCreatePlayerRequestHandler : IIORequestHandler
    {
        public IOCreatePlayerRequestHandler()
        {
        }
        /// <summary>
        /// Register to handle a specific set of IO requests
        /// </summary>
        /// <returns></returns>
        public List<string> RegisterForIORequests()
        {
            return new List<string>()
            {
                typeof(CreatePlayerRequest).ToString()
            };
        }
        /// <summary>
        /// Handle terminal request
        /// </summary>
        /// <param name="api"></param>
        /// <param name="terminal"></param>
        /// <param name="terminalRequest"></param>
        public IResponse HandleIORequest(IApi api, Terminal terminal, IRequest request, IResponse response)
        {
            if (request.IsMessageType<CreatePlayerRequest>())
            {
                var createPlayerRequest = request.GetMessageAsType<CreatePlayerRequest>();
                var player = api.GetAttributeManager<IAttributePlayerManager>().Create(
                    createPlayerRequest.Name,
                    terminal.AccountId,
                    new AttributePhysical()
                    {
                        Name = createPlayerRequest.Name,
                        Description = "A player",
                        Weight = 100
                    },
                    api.GetAttributeManager<IAttributeGameManager>().GetDefaultLocation());
                var createPlayerResponse = new CreatePlayerResponse()
                {
                    PlayerCreatedEvent = new PlayerCreatedEvent()
                    {
                        ItemId = player.ItemId,
                        Name = player.Name
                    }
                };
                response = createPlayerResponse;
            }
            return response;
        }
    }
}
