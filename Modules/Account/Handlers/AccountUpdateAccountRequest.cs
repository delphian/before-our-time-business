using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Account.Managers;
using BeforeOurTime.Models.Modules.Account.Messages.UpdateAccount;
using BeforeOurTime.Models.Modules.Account.Models.Data;
using BeforeOurTime.Models.Terminals;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Modules.Account.Managers
{
    public partial class AccountManager
    {
        /// <summary>
        /// Update account endpoint
        /// </summary>
        /// <param name="message"></param>
        /// <param name="mm">Module manager</param>
        /// <param name="terminal">Terminal that initiated request</param>
        /// <param name="response"></param>
        public IResponse HandleUpdateAccountRequest(
            IMessage message,
            IModuleManager mm,
            ITerminal terminal,
            IResponse response)
        {
            var request = message.GetMessageAsType<AccountUpdateAccountRequest>();
            response = HandleRequestWrapper<AccountUpdateAccountResponse>(request, res =>
            {
                var accountManager = mm.GetManager<IAccountManager>();
                var accountData = new AccountData()
                {
                    Id = request.Account.Id,
                    Name = request.Account.Name,
                    Password = request.Account.Password,
                    Temporary = request.Account.Temporary
                };
                accountManager.Update(accountData);
                ((AccountUpdateAccountResponse)res).UpdateAccountEvent = new AccountUpdateAccountEvent()
                {
                    Account = request.Account
                };
                res.SetSuccess(true);
            });
            return response;
        }
    }
}
