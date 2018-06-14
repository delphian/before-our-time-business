﻿using BeforeOurTime.Business.Apis.Items.Attributes.Interfaces;
using BeforeOurTime.Business.Apis.Scripts.Delegates.OnTerminalInput;
using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Attributes;
using BeforeOurTime.Repository.Models.Items.Attributes.Exits;
using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Messages.Events.Emotes;
using BeforeOurTime.Repository.Models.Messages.Requests;
using BeforeOurTime.Repository.Models.Messages.Requests.Look;
using BeforeOurTime.Repository.Models.Messages.Responses;
using BeforeOurTime.Repository.Models.Messages.Responses.Enumerate;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO.Requests.Handlers
{
    public class EmoteRequestHandler : IRequestHandler
    {
        public EmoteRequestHandler()
        {
        }
        /// <summary>
        /// Register to handle a specific set of IO requests
        /// </summary>
        /// <returns></returns>
        public List<string> RegisterForRequests()
        {
            return new List<string>()
            {
                typeof(EmoteRequest).ToString()
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
            if (request.IsMessageType<EmoteRequest>())
            {
                var emoteRequest = request.GetMessageAsType<EmoteRequest>();
                var player = api.GetItemManager().Read(terminal.PlayerId);
                var location = api.GetItemManager().ReadWithChildren(player.ParentId.Value);
                api.GetMessageManager().SendMessageToLocation(new EmoteEvent()
                    {
                        ItemId = player.Id,
                        Name = player.GetAttribute<AttributePlayer>().Name,
                        Type = emoteRequest.Type
                    }, location, player.Id);
                response = new Response() { ResponseSuccess = true };
            }
            return response;
        }
    }
}