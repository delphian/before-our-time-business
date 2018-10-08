using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules.Account.Messages.LoginAccount;
using BeforeOurTime.Models.Terminals;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Modules.Account.Managers
{
    public partial class AccountManager
    {
        /// <summary>
        /// Handle a message
        /// </summary>
        /// <param name="api"></param>
        /// <param name="message"></param>
        /// <param name="terminal"></param>
        /// <param name="response"></param>
        public IResponse HandleLoginAccountRequest(IMessage message, IApi api, ITerminal terminal, IResponse response)
        {
            var request = message.GetMessageAsType<AccountLoginAccountRequest>();
            response = HandleRequestWrapper<AccountLoginAccountResponse>(request, res =>
            {
                var account = Authenticate(request.Email, request.Password);
                ((AccountLoginAccountResponse)res).AccountId = account?.Id;
                res.SetSuccess(account?.Id != null);
            });
            return response;
        }
    }
}
