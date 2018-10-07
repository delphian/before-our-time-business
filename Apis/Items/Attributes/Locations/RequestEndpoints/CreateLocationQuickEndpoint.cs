using BeforeOurTime.Business.Apis.Messages.RequestEndpoints;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.ItemAttributes;
using BeforeOurTime.Models.Messages.Locations.CreateLocation;
using BeforeOurTime.Models.Messages.Locations.Locations.CreateLocation;
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
using BeforeOurTime.Models.Modules.Core.Managers;

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
                CreateLocationQuickRequest._Id
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
            if (request.GetType() == typeof(CreateLocationQuickRequest))
            {
                var createLocationQuickRequest = request.GetMessageAsType<CreateLocationQuickRequest>();
                var player = api.GetItemManager().Read(terminal.GetPlayerId().Value);
                var location = api.GetItemManager().Read(player.ParentId.Value);
                var createFromLocationItemId = createLocationQuickRequest.FromLocationItemId ??
                                               player.ParentId.Value;
                var newLocationItem = api
                    .GetModuleManager()
                    .GetModule<ICoreModule>()
                    .GetManager<ILocationItemManager>()
                    .CreateFromHere(createFromLocationItemId);
                response = new CreateLocationQuickResponse()
                {
                    _requestInstanceId = request.GetRequestInstanceId(),
                    _responseSuccess = true,
                    CreateLocationEvent = new CreateLocationEvent()
                    {
                        Item = newLocationItem
                    }
                };
            }
            return response;
        }
    }
}
