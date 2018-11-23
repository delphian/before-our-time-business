using BeforeOurTime.Models.Exceptions;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Account.Dbs;
using BeforeOurTime.Models.Modules.Account.Managers;
using BeforeOurTime.Models.Modules.Account.Messages.Json;
using BeforeOurTime.Models.Modules.Account.Messages.Json.ReadAccount;
using BeforeOurTime.Models.Modules.Account.Messages.Json.RestoreAccount;
using BeforeOurTime.Models.Modules.Account.Models.Data;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Terminal.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Modules.Account.Managers
{
    public partial class AccountManager
    {
        /// <summary>
        /// Erase all accounts and restore from backup json data
        /// </summary>
        /// <param name="message"></param>
        /// <param name="origin">Item that initiated request</param>
        /// <param name="mm">Module manager</param>
        /// <param name="response"></param>
        public IResponse HandleJsonRestoreAccountRequest(
            IMessage message,
            Item origin,
            IModuleManager mm,
            IResponse response)
        {
            var request = message.GetMessageAsType<AccountJsonRestoreAccountRequest>();
            response = HandleRequestWrapper<AccountJsonRestoreAccountResponse>(request, res =>
            {
                var accountDataRepo = GetRepository<IAccountDataRepo>();
                if (!origin.HasData<AccountData>() || !origin.GetData<AccountData>().Admin)
                {
                    throw new BotAuthorizationDeniedException("Permission denied");
                }
                var accountDatas = JsonConvert.DeserializeObject<List<AccountData>>(request.AccountsJson);
                accountDataRepo.Delete();
                accountDatas.ForEach(accountData =>
                {
                    accountDataRepo.Create(accountData);
                });
                res.SetSuccess(true);
            });
            return response;
        }
    }
}
