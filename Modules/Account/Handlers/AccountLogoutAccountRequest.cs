using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules.Account.Messages.LogoutAccount;
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
        public IResponse HandleLogoutAccountRequest(IMessage message, IApi api, ITerminal terminal, IResponse response)
        {
            var request = message.GetMessageAsType<AccountLogoutAccountRequest>();
            response = HandleRequestWrapper<AccountLogoutAccountResponse>(request, res =>
            {
            });
            return response;
        }
    }
}
