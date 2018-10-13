using BeforeOurTime.Business.Apis.Messages.RequestEndpoints;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Requests.Go;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules.Core;
using BeforeOurTime.Models.Modules.World.Dbs;
using BeforeOurTime.Models.Modules.World.Managers;
using BeforeOurTime.Models.Modules.World.Messages.Location.ReadLocationSummary;
using BeforeOurTime.Models.Modules.World.Models.Data;
using BeforeOurTime.Models.Terminals;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Exits.RequestEndpoints
{
    public class UseExitEndpoint : IRequestEndpoint
    {
        public UseExitEndpoint()
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
                GoRequest._Id
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
            if (request.GetType() == typeof(GoRequest))
            {
                var goRequest = request.GetMessageAsType<GoRequest>();
                var exit = api.GetItemManager().Read(goRequest.ItemId);
                var player = api.GetItemManager().Read(terminal.GetPlayerId().Value);
                var locationAttribute = api
                    .GetModuleManager()
                    .GetModule<ICoreModule>()
                    .GetManager<ILocationItemManager>()
                    .GetRepository<ILocationDataRepo>()
                    .Read(exit.GetAttribute<ExitData>().DestinationLocationId);
                var location = api.GetItemManager().Read(locationAttribute.DataItemId);
                api.GetItemManager().Move(player, location, exit);
                var lookRequestHandler = new WorldReadLocationSummaryRequest();
                response = api.GetModuleManager().GetManager<ILocationItemManager>()
                    .HandleReadLocationSummaryRequest(new WorldReadLocationSummaryRequest()
                    {
                    }, api, terminal, response);
            }
            return response;
        }
    }
}
