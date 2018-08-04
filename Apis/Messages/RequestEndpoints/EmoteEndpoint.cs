using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Business.Apis.Messages.RequestEndpoints;
using BeforeOurTime.Business.Apis.Scripts.Delegates.OnTerminalInput;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Models.Items.Attributes.Players;
using BeforeOurTime.Models.Messages.Events.Emotes;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Requests.Emote;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Repository.Models.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.Messages.RequestEndpoints
{
    public class EmoteEndpoint : IRequestEndpoint
    {
        public EmoteEndpoint()
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
                EmoteRequest._Id
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
                var player = api.GetItemManager().Read(terminal.PlayerId.Value);
                var location = api.GetItemManager().ReadWithChildren(player.ParentId.Value);
                api.GetMessageManager().SendMessageToLocation(new EmoteEvent()
                    {
                        Item = player,
                        Name = player.GetAttribute<PlayerAttribute>().Name,
                        Type = emoteRequest.Type
                    }, location, player.Id);
                response = new Response() {
                    _requestInstanceId = request.GetRequestInstanceId(),
                    ResponseSuccess = true
                };
            }
            return response;
        }
    }
}
