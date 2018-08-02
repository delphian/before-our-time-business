using BeforeOurTime.Business.Apis.IO.Requests.Handlers;
using BeforeOurTime.Business.Apis.Items.Attributes.Interfaces;
using BeforeOurTime.Business.Apis.Items.Attributes.Locations;
using BeforeOurTime.Business.Apis.Scripts.Delegates.OnTerminalInput;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Items.Attributes;
using BeforeOurTime.Models.Items.Attributes.Characters;
using BeforeOurTime.Models.Items.Attributes.Players;
using BeforeOurTime.Models.Messages.CRUD.Items.CreateItem;
using BeforeOurTime.Models.Messages.CRUD.Items.ReadItem;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Requests.List;
using BeforeOurTime.Models.Messages.Requests.LocationAttributes;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Messages.Responses.List;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Locations.RequestEndpoints
{
    public class ReadItemEndpoint : IRequestHandler
    {
        public ReadItemEndpoint()
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
                ReadItemRequest._Id
            };
        }
        /// <summary>
        /// Handle terminal request
        /// </summary>
        /// <param name="api"></param>
        /// <param name="terminal"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        public IResponse HandleRequest(IApi api, Terminal terminal, IRequest request, IResponse response)
        {
            if (request.GetType() == typeof(ReadItemRequest))
            {
                var readItemRequest = request.GetMessageAsType<ReadItemRequest>();
                var player = api.GetItemManager().Read(terminal.PlayerId.Value);
                response = new ReadItemResponse()
                {
                    ResponseSuccess = true,
                    ReadItemEvent = new ReadItemEvent()
                    {
                        Item = api.GetItemManager().Read(readItemRequest.ItemId)
                    }
                };
            }
            return response;
        }
    }
}
