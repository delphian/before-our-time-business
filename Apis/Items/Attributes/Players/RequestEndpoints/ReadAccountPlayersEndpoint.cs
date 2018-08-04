using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Business.Apis.Items.Attributes.Players;
using BeforeOurTime.Business.Apis.Messages.RequestEndpoints;
using BeforeOurTime.Business.Apis.Scripts.Delegates.OnTerminalInput;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Models.Items.Attributes;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Requests.List;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Messages.Responses.List;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Players.RequestEndpoints
{
    public class ReadAccountPlayersEndpoint : IRequestEndpoint
    {
        public ReadAccountPlayersEndpoint()
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
                ListAccountCharactersRequest._Id
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
            if (request.IsMessageType<ListAccountCharactersRequest>())
            {
                var listAccountCharactersRequest = request.GetMessageAsType<ListAccountCharactersRequest>();
                var itemIds = api.GetAttributeManager<IPlayerAttributeManager>()
                    .Read()
                    .Where(x => x.AccountId == listAccountCharactersRequest.AccountId)
                    .Select(x => x.ItemId)
                    .ToList();
                var items = api.GetItemManager().Read(itemIds);
                var listAccountCharactersResponse = new ListAccountCharactersResponse()
                {
                    _requestInstanceId = request.GetRequestInstanceId(),
                    _responseSuccess = true,
                    AccountCharacters = items
                };
                response = listAccountCharactersResponse;
            }
            return response;
        }
    }
}
