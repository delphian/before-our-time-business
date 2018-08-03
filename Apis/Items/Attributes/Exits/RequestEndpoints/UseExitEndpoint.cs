﻿using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Business.Apis.Items.Attributes.Locations;
using BeforeOurTime.Business.Apis.Items.Attributes.Locations.RequestEndpoints;
using BeforeOurTime.Business.Apis.Messages.RequestEndpoints;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Models.Items.Attributes;
using BeforeOurTime.Models.Items.Attributes.Exits;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Requests.Go;
using BeforeOurTime.Models.Messages.Requests.List;
using BeforeOurTime.Models.Messages.Responses;
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
        public IResponse HandleRequest(IApi api, Terminal terminal, IRequest request, IResponse response)
        {
            if (request.GetType() == typeof(GoRequest))
            {
                var goRequest = request.GetMessageAsType<GoRequest>();
                var exit = api.GetItemManager().Read(goRequest.ItemId);
                var player = api.GetItemManager().Read(terminal.PlayerId.Value);
                var locationAttribute = api.GetAttributeManager<ILocationAttributeManager>()
                    .Read(exit.GetAttribute<ExitAttribute>().DestinationLocationId);
                var location = api.GetItemManager().Read(locationAttribute.ItemId);
                api.GetItemManager().Move(player, location, exit);
                var lookRequestHandler = new ExamineLocationEndpoint();
                response = lookRequestHandler.HandleRequest(api, terminal, new ListLocationRequest()
                {

                }, response);
            }
            return response;
        }
    }
}