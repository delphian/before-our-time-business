﻿using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Business.Apis.Items.Attributes.Exits;
using BeforeOurTime.Business.Apis.Items.Attributes.Locations;
using BeforeOurTime.Business.Apis.Messages.RequestEndpoints;
using BeforeOurTime.Business.Apis.Scripts.Delegates.OnTerminalInput;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Items.Attributes;
using BeforeOurTime.Models.Items.Attributes.Characters;
using BeforeOurTime.Models.Items.Attributes.Players;
using BeforeOurTime.Models.Messages.CRUD.Items.DeleteItem;
using BeforeOurTime.Models.Messages.Locations.CreateLocation;
using BeforeOurTime.Models.Messages.Locations.DeleteLocation;
using BeforeOurTime.Models.Messages.Locations.Locations.CreateLocation;
using BeforeOurTime.Models.Messages.Locations.Locations.DeleteLocation;
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
    public class DeleteLocationEndpoint : IRequestEndpoint
    {
        public DeleteLocationEndpoint()
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
                DeleteLocationRequest._Id
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
            if (request.GetType() == typeof(DeleteLocationRequest))
            {
                response = new DeleteLocationResponse()
                {
                    _responseSuccess = false,
                    _requestInstanceId = request.GetRequestInstanceId()
                };
            }
            try
            {
                var deleteLocationRequest = request.GetMessageAsType<DeleteLocationRequest>();
                var player = api.GetItemManager().Read(terminal.PlayerId.Value);
                var location = api.GetItemManager().Read(deleteLocationRequest.LocationItemId);
                var exits = api.GetAttributeManager<IExitAttributeManager>()
                    .GetLocationExits(location);
                api.GetItemManager().Delete(exits, true);
                location = api.GetItemManager().Read(deleteLocationRequest.LocationItemId);
                api.GetItemManager().Delete(new List<Item>() { location });
                var deletedItems = exits ?? new List<Item>();
                deletedItems.Add(location);
                var deleteItemEvent = new DeleteItemEvent()
                {
                    Items = deletedItems
                };
                ((DeleteLocationResponse)response).DeleteItemEvent = deleteItemEvent;
                ((DeleteLocationResponse)response)._responseSuccess = true;
            }
            catch (Exception e)
            {
                var traverseExceptions = e;
                while (traverseExceptions != null)
                {
                    ((DeleteLocationResponse)response)._responseMessage += traverseExceptions.Message + ". ";
                    traverseExceptions = traverseExceptions.InnerException;
                }
                api.GetLogger().Log(LogLevel.Error, ((DeleteLocationResponse)response)._responseMessage);
            }
            return response;
        }
    }
}
