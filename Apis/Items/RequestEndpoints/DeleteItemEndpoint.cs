using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Business.Apis.Items.Attributes.Locations;
using BeforeOurTime.Business.Apis.Messages.RequestEndpoints;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Models;
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
using BeforeOurTime.Models.Modules.Core.Messages.ItemCrud.DeleteItem;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Locations.RequestEndpoints
{
    public class DeleteItemEndpoint : IRequestEndpoint
    {
        public DeleteItemEndpoint()
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
                CoreDeleteItemCrudRequest._Id
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
            if (request.GetType() == typeof(CoreDeleteItemCrudRequest))
            {
                response = new CoreDeleteItemCrudResponse()
                {
                    _responseSuccess = false,
                    _requestInstanceId = request.GetRequestInstanceId()
                };
                try
                {
                    var deleteItemRequest = request.GetMessageAsType<CoreDeleteItemCrudRequest>();
                    var player = api.GetItemManager().Read(terminal.GetPlayerId().Value);
                    var items = api.GetItemManager().Read(deleteItemRequest.ItemIds);
                    api.GetItemManager().Delete(items, true);
                    var deleteItemEvent = new CoreDeleteItemCrudEvent()
                        {
                            Items = items
                        };
                    api.GetMessageManager().SendMessageToLocation(deleteItemEvent, player.Parent, player.Id);
                    ((CoreDeleteItemCrudResponse)response).CoreDeleteItemCrudEvent = deleteItemEvent;
                    ((CoreDeleteItemCrudResponse)response)._responseSuccess = true;
                }
                catch (Exception e)
                {
                    var traverseExceptions = e;
                    while(traverseExceptions != null)
                    {
                        ((CoreDeleteItemCrudResponse)response)._responseMessage += traverseExceptions.Message + ". ";
                        traverseExceptions = traverseExceptions.InnerException;
                    }
                    api.GetLogger().Log(LogLevel.Error, ((CoreDeleteItemCrudResponse)response)._responseMessage);
                }
            }
            return response;
        }
    }
}
