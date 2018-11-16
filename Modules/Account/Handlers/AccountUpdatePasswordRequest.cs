using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Account.Dbs;
using BeforeOurTime.Models.Modules.Account.Managers;
using BeforeOurTime.Models.Modules.Account.Messages.UpdateAccount;
using BeforeOurTime.Models.Modules.Account.Messages.UpdatePassword;
using BeforeOurTime.Models.Modules.Account.Models.Data;
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
        /// Update account password
        /// </summary>
        /// <param name="message"></param>
        /// <param name="origin">Item that initiated request</param>
        /// <param name="mm">Module manager</param>
        /// <param name="response"></param>
        public IResponse HandleUpdatePasswordRequest(
            IMessage message,
            Item origin,
            IModuleManager mm,
            IResponse response)
        {
            var request = message.GetMessageAsType<AccountUpdatePasswordRequest>();
            response = HandleRequestWrapper<AccountUpdatePasswordResponse>(request, res =>
            {
                var accountManager = mm.GetManager<IAccountManager>();
                var accountData = accountManager.GetRepository<IAccountDataRepo>()
                    .Read(request.AccountId);
                if (origin.GetData<AccountData>().Id != request.AccountId ||
                    accountManager.Authenticate(accountData.Name, request.OldPassword) == null)
                {
                    res.SetSuccess(false).SetMessage("Permission Denied");
                }
                else
                {
                    accountData.Password = request.NewPassword;
                    accountManager.Update(accountData);
                    ((AccountUpdatePasswordResponse)res).Account = new Models.Modules.Account.Models.Account()
                    {
                        Id = accountData.Id,
                        Name = accountData.Name,
                        Password = null,
                        Temporary = accountData.Temporary,
                        Admin = accountData.Admin
                    };
                    res.SetSuccess(true);
                }
            });
            return response;
        }
    }
}
