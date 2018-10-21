using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules.Account.Messages.CreateAccount;
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
        public IResponse HandleCreateAccountRequest(IMessage message, IApi api, ITerminal terminal, IResponse response)
        {
            var request = message.GetMessageAsType<AccountCreateAccountRequest>();
            response = HandleRequestWrapper<AccountCreateAccountResponse>(request, res =>
            {
                var account = Create(request.Email, request.Password, request.Temporary);
                ((AccountCreateAccountResponse)res).CreatedAccountEvent = new AccountCreateAccountEvent()
                {
                    AccountId = account.Id
                };
                res.SetSuccess(true);
            });
            return response;
        }
    }
}
