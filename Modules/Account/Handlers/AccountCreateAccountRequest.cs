using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Account.Messages.CreateAccount;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Terminal.Models;
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
        /// <param name="message"></param>
        /// <param name="origin">Item that initiated request</param>
        /// <param name="mm">Module manager</param>
        /// <param name="response"></param>
        public IResponse HandleCreateAccountRequest(
            IMessage message,
            Item origin,
            IModuleManager mm,
            IResponse response)
        {
            var request = message.GetMessageAsType<AccountCreateAccountRequest>();
            response = HandleRequestWrapper<AccountCreateAccountResponse>(request, res =>
            {
                var account = Create(request.Email, request.Password, request.Temporary);
                ((AccountCreateAccountResponse)res).CreatedAccountEvent = new AccountCreateAccountEvent()
                {
                    Account = new Models.Modules.Account.Models.Account()
                    {
                        Id = account.Id,
                        Password = null,
                        Name = account.Name,
                        Temporary = account.Temporary
                    }
                };
                res.SetSuccess(true);
            });
            return response;
        }
    }
}
