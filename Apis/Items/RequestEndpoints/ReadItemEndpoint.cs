using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Business.Apis.Items.Attributes.Locations;
using BeforeOurTime.Business.Apis.Messages.RequestEndpoints;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.ItemAttributes;
using BeforeOurTime.Models.ItemAttributes.Characters;
using BeforeOurTime.Models.ItemAttributes.Players;
using BeforeOurTime.Models.Messages.CRUD.Items.CreateItem;
using BeforeOurTime.Models.Messages.CRUD.Items.ReadItem;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Requests.List;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Messages.Responses.List;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Terminals;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Locations.RequestEndpoints
{
    public class ReadItemEndpoint : IRequestEndpoint
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
        public IResponse HandleRequest(IApi api, ITerminal terminal, IRequest request, IResponse response)
        {
            if (request.GetType() == typeof(ReadItemRequest))
            {
                var readItemRequest = request.GetMessageAsType<ReadItemRequest>();
                var player = api.GetItemManager().Read(terminal.GetPlayerId().Value);
                var items = new List<Item>();
                // Read enumerated list of items
                if (readItemRequest.ItemIds?.FirstOrDefault() != null)
                {
                    items = api.GetItemManager().Read(readItemRequest.ItemIds);
                }
                // Read items of type
                if (readItemRequest.ItemAttributeTypes?.FirstOrDefault() != null)
                {
                    var attributeManager = api.GetAttributeManagerOfType(readItemRequest.ItemAttributeTypes.First());
                    items = attributeManager.ReadItem();
                }
                // Read all items
                if (readItemRequest.ItemIds?.FirstOrDefault() == null &&
                    readItemRequest.ItemAttributeTypes?.FirstOrDefault() == null)
                {
                    items = api.GetItemManager().Read();
                }
                response = new ReadItemResponse()
                {
                    _requestInstanceId = request.GetRequestInstanceId(),
                    _responseSuccess = true,
                    ReadItemEvent = new ReadItemEvent()
                    {
                        Items = items
                    }
                };
            }
            return response;
        }
    }
}
