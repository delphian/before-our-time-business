using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Business.Apis.Messages.RequestEndpoints;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Requests.List;
using BeforeOurTime.Models.Messages.Requests.Login;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Messages.Responses.List;
using BeforeOurTime.Models.Messages.Responses.Login;
using BeforeOurTime.Models.Terminals;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.Messages.RequestEndpoints
{
    public class LoginAccountEndpoint : IRequestEndpoint
    {
        public LoginAccountEndpoint()
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
                LoginRequest._Id,
                LogoutRequest._Id
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
            if (request.IsMessageType<LoginRequest>())
            {
                var loginRequest = request.GetMessageAsType<LoginRequest>();
                var account = api.GetAccountManager().Authenticate(loginRequest.Email, loginRequest.Password);
                response = new LoginResponse()
                {
                    _requestInstanceId = request.GetRequestInstanceId(),
                    _responseSuccess = (account != null),
                    AccountId = account?.Id
                };
            }
            if (request.IsMessageType<LogoutRequest>())
            {
                response = new LogoutResponse()
                {
                    _requestInstanceId = request.GetRequestInstanceId(),
                    _responseSuccess = true
                };
            };
            return response;
        }
    }
}
