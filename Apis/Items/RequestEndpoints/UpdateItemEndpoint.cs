using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Business.Apis.Items.Attributes.Locations;
using BeforeOurTime.Business.Apis.Messages.RequestEndpoints;
using BeforeOurTime.Business.Apis.Scripts.Delegates.OnTerminalInput;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Items.Attributes;
using BeforeOurTime.Models.Items.Attributes.Characters;
using BeforeOurTime.Models.Items.Attributes.Players;
using BeforeOurTime.Models.Messages.CRUD.Items.CreateItem;
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
                UpdateItemRequest._Id
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
            if (request.GetType() == typeof(UpdateItemRequest))
            {
                response = new UpdateItemResponse()
                {
                    _responseSuccess = false,
                    _requestInstanceId = request.GetRequestInstanceId()
                };
                try
                {
                    var updateItemRequest = request.GetMessageAsType<UpdateItemRequest>();
                    var player = api.GetItemManager().Read(terminal.PlayerId.Value);
                    api.GetItemManager().Update(updateItemRequest.Items);
                    var updateItemEvent = new UpdateItemEvent()
                    {
                        Items = updateItemRequest.Items
                    };
                    api.GetMessageManager().SendMessageToLocation(updateItemEvent, player.Parent, player.Id);
                    ((UpdateItemResponse)response).UpdateItemEvent = updateItemEvent;
                    ((UpdateItemResponse)response)._responseSuccess = true;
                }
                catch (Exception e)
                {
                    var traverseExceptions = e;
                    while (traverseExceptions != null)
                    {
                        ((UpdateItemResponse)response)._responseMessage += traverseExceptions.Message + ". ";
                        traverseExceptions = traverseExceptions.InnerException;
                    }
                    api.GetLogger().Log(LogLevel.Error, ((UpdateItemResponse)response)._responseMessage);
                }
            }
            return response;
        }
    }
}
