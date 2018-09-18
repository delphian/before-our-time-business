using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Business.Apis.Items.Attributes.Locations;
using BeforeOurTime.Business.Apis.Messages.RequestEndpoints;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.ItemAttributes;
using BeforeOurTime.Models.ItemAttributes.Characters;
using BeforeOurTime.Models.ItemAttributes.Players;
using BeforeOurTime.Models.Messages.CRUD.Items.CreateItem;
using BeforeOurTime.Models.Messages.CRUD.Items.DeleteItem;
using BeforeOurTime.Models.Messages.CRUD.Items.ReadItem;
using BeforeOurTime.Models.Messages.CRUD.Items.UpdateItem;
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
                DeleteItemRequest._Id
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
            if (request.GetType() == typeof(DeleteItemRequest))
            {
                response = new DeleteItemResponse()
                {
                    _responseSuccess = false,
                    _requestInstanceId = request.GetRequestInstanceId()
                };
                try
                {
                    var deleteItemRequest = request.GetMessageAsType<DeleteItemRequest>();
                    var player = api.GetItemManager().Read(terminal.GetPlayerId().Value);
                    var items = api.GetItemManager().Read(deleteItemRequest.ItemIds);
                    api.GetItemManager().Delete(items, true);
                    var deleteItemEvent = new DeleteItemEvent()
                        {
                            Items = items
                        };
                    api.GetMessageManager().SendMessageToLocation(deleteItemEvent, player.Parent, player.Id);
                    ((DeleteItemResponse)response).DeleteItemEvent = deleteItemEvent;
                    ((DeleteItemResponse)response)._responseSuccess = true;
                }
                catch (Exception e)
                {
                    var traverseExceptions = e;
                    while(traverseExceptions != null)
                    {
                        ((DeleteItemResponse)response)._responseMessage += traverseExceptions.Message + ". ";
                        traverseExceptions = traverseExceptions.InnerException;
                    }
                    api.GetLogger().Log(LogLevel.Error, ((DeleteItemResponse)response)._responseMessage);
                }
            }
            return response;
        }
    }
}
