using BeforeOurTime.Models.Exceptions;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Account.Dbs;
using BeforeOurTime.Models.Modules.Account.Managers;
using BeforeOurTime.Models.Modules.Account.Messages.DeleteAccount;
using BeforeOurTime.Models.Modules.Account.Messages.ReadAccount;
using BeforeOurTime.Models.Modules.Account.Messages.UpdateAccount;
using BeforeOurTime.Models.Modules.Account.Models.Data;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Terminal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Modules.Account.Managers
{
    public partial class AccountManager
    {
        /// <summary>
        /// Delete an account endpoint
        /// </summary>
        /// <param name="message"></param>
        /// <param name="origin">Item that initiated request</param>
        /// <param name="mm">Module manager</param>
        /// <param name="response"></param>
        public IResponse HandleDeleteAccountRequest(
            IMessage message,
            Item origin,
            IModuleManager mm,
            IResponse response)
        {
            var request = message.GetMessageAsType<AccountDeleteAccountRequest>();
            response = HandleRequestWrapper<AccountDeleteAccountResponse>(request, res =>
            {
                var accountManager = mm.GetManager<IAccountManager>();
                if (!origin.HasData<AccountData>() || !origin.GetData<AccountData>().Admin)
                {
                    throw new BotAuthorizationDeniedException("Permission denied");
                }
                accountManager.Delete(request.AccountId);
                res.SetSuccess(true);
            });
            return response;
        }
    }
}
