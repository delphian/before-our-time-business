using BeforeOurTime.Business.Apis.Items.Attributes.Interfaces;
using BeforeOurTime.Business.Apis.Scripts.Delegates.OnTerminalInput;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Models.Messages.Events.Created;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Requests.Create;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Messages.Responses.Create;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Attributes;
using BeforeOurTime.Repository.Models.Items.Attributes.Exits;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO.Requests.Handlers
{
    public class CreatePlayerRequestHandler : IRequestHandler
    {
        public CreatePlayerRequestHandler()
        {
        }
        /// <summary>
        /// Register to handle a specific set of IO requests
        /// </summary>
        /// <returns></returns>
        public List<string> RegisterForRequests()
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
        public IResponse HandleRequest(IApi api, Terminal terminal, IRequest request, IResponse response)
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
                    ResponseSuccess = true,
                    CreatedPlayerEvent = new CreatedPlayerEvent()
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
