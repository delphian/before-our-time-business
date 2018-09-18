using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Business.Apis.Messages.RequestEndpoints;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.ItemAttributes;
using BeforeOurTime.Models.Messages.Events.Created;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Requests.Create;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Messages.Responses.Create;
using BeforeOurTime.Models.Terminals;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.Messages.RequestEndpoints
{
    public class CreateAccountEndpoint : IRequestEndpoint
    {
        public CreateAccountEndpoint()
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
                CreateAccountRequest._Id
            };
        }
        /// <summary>
        /// Handle terminal request
        /// </summary>
        /// <param name="api"></param>
        /// <param name="terminal"></param>
        /// <param name="terminalRequest"></param>
        public IResponse HandleRequest(IApi api, ITerminal terminal, IRequest request, IResponse response)
        {
            if (request.IsMessageType<CreateAccountRequest>())
            {
                var createAccountRequest = request.GetMessageAsType<CreateAccountRequest>();
                var account = api.GetAccountManager().Create(
                    createAccountRequest.Email,
                    createAccountRequest.Email,
                    createAccountRequest.Password);
                var createAccountResponse = new CreateAccountResponse()
                {
                    _requestInstanceId = request.GetRequestInstanceId(),
                    _responseSuccess = true,
                    CreatedAccountEvent = new CreatedAccountEvent()
                    {
                        AccountId = account.Id
                    }
                };
                response = createAccountResponse;
            }
            return response;
        }
    }
}
