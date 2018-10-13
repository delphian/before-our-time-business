using BeforeOurTime.Business.Apis.Messages.RequestEndpoints;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Terminals;
using BeforeOurTime.Models.Modules.Core;
using BeforeOurTime.Models.Modules.World.Messages.Location.CreateLocation;
using BeforeOurTime.Models.Modules.World.Managers;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Locations.RequestEndpoints
{
    public class CreateLocationQuickEndpoint : IRequestEndpoint
    {
        public CreateLocationQuickEndpoint()
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
                WorldCreateLocationQuickRequest._Id
            };
        }
        /// <summary>
        /// Handle terminal request
        /// </summary>
        /// <param name="api"></param>
        /// <param name="terminal"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        public IResponse HandleRequest(IApi api, ITerminal terminal, IRequest request, IResponse response)
        {
            if (request.GetType() == typeof(WorldCreateLocationQuickRequest))
            {
                var createLocationQuickRequest = request.GetMessageAsType<WorldCreateLocationQuickRequest>();
                var player = api.GetItemManager().Read(terminal.GetPlayerId().Value);
                var location = api.GetItemManager().Read(player.ParentId.Value);
                var createFromLocationItemId = createLocationQuickRequest.FromLocationItemId ??
                                               player.ParentId.Value;
                var newLocationItem = api
                    .GetModuleManager()
                    .GetModule<ICoreModule>()
                    .GetManager<ILocationItemManager>()
                    .CreateFromHere(createFromLocationItemId);
                response = new WorldCreateLocationQuickResponse()
                {
                    _requestInstanceId = request.GetRequestInstanceId(),
                    _responseSuccess = true,
                    CreateLocationEvent = new WorldCreateLocationEvent()
                    {
                        Item = newLocationItem
                    }
                };
            }
            return response;
        }
    }
}
