using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Business.Apis.Items.Attributes.Locations;
using BeforeOurTime.Business.Apis.Messages.RequestEndpoints;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.ItemAttributes;
using BeforeOurTime.Models.ItemAttributes.Characters;
using BeforeOurTime.Models.ItemAttributes.Players;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Requests.List;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Messages.Responses.List;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Terminals;
using BeforeOurTime.Models.Modules.Core.Messages.ItemCrud.UpdateItem;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Locations.RequestEndpoints
{
    public class UpdateItemEndpoint : IRequestEndpoint
    {
        public UpdateItemEndpoint()
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
                CoreUpdateItemCrudRequest._Id
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
            if (request.GetType() == typeof(CoreUpdateItemCrudRequest))
            {
                response = new CoreUpdateItemCrudResponse()
                {
                    _responseSuccess = false,
                    _requestInstanceId = request.GetRequestInstanceId()
                };
                try
                {
                    var updateItemRequest = request.GetMessageAsType<CoreUpdateItemCrudRequest>();
                    var player = api.GetItemManager().Read(terminal.GetPlayerId().Value);
                    api.GetItemManager().Update(updateItemRequest.Items);
                    var updateItemEvent = new CoreUpdateItemCrudEvent()
                    {
                        Items = updateItemRequest.Items
                    };
                    api.GetMessageManager().SendMessageToLocation(updateItemEvent, player.Parent, player.Id);
                    ((CoreUpdateItemCrudResponse)response).CoreUpdateItemCrudEvent = updateItemEvent;
                    ((CoreUpdateItemCrudResponse)response)._responseSuccess = true;
                }
                catch (Exception e)
                {
                    var traverseExceptions = e;
                    while (traverseExceptions != null)
                    {
                        ((CoreUpdateItemCrudResponse)response)._responseMessage += traverseExceptions.Message + ". ";
                        traverseExceptions = traverseExceptions.InnerException;
                    }
                    api.GetLogger().Log(LogLevel.Error, ((CoreUpdateItemCrudResponse)response)._responseMessage);
                }
            }
            return response;
        }
    }
}
