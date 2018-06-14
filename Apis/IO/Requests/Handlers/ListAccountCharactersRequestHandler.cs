using BeforeOurTime.Business.Apis.Items.Attributes.Interfaces;
using BeforeOurTime.Business.Apis.Scripts.Delegates.OnTerminalInput;
using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Attributes;
using BeforeOurTime.Repository.Models.Items.Attributes.Exits;
using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Messages.Events;
using BeforeOurTime.Repository.Models.Messages.Requests;
using BeforeOurTime.Repository.Models.Messages.Requests.List;
using BeforeOurTime.Repository.Models.Messages.Requests.Look;
using BeforeOurTime.Repository.Models.Messages.Responses;
using BeforeOurTime.Repository.Models.Messages.Responses.Create;
using BeforeOurTime.Repository.Models.Messages.Responses.Enumerate;
using BeforeOurTime.Repository.Models.Messages.Responses.List;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO.Requests.Handlers
{
    public class ListAccountCharactersRequestHandler : IRequestHandler
    {
        public ListAccountCharactersRequestHandler()
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
                typeof(ListAccountCharactersRequest).ToString()
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
                var itemIds = api.GetAttributeManager<IAttributePlayerManager>()
                    .Read()
                    .Where(x => x.AccountId == listAccountCharactersRequest.AccountId)
                    .Select(x => x.ItemId)
                    .ToList();
                var listAccountCharactersResponse = new ListAccountCharactersResponse()
                {
                    ResponseSuccess = true,
                    AccountCharacters = api.GetItemManager().Read(itemIds)
                };
                response = listAccountCharactersResponse;
            }
            return response;
        }
    }
}
