using BeforeOurTime.Models.Exceptions;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Account.Dbs;
using BeforeOurTime.Models.Modules.Account.Managers;
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
        /// Read account data endpoint
        /// </summary>
        /// <param name="message"></param>
        /// <param name="origin">Item that initiated request</param>
        /// <param name="mm">Module manager</param>
        /// <param name="response"></param>
        public IResponse HandleReadAccountRequest(
            IMessage message,
            Item origin,
            IModuleManager mm,
            IResponse response)
        {
            var request = message.GetMessageAsType<AccountReadAccountRequest>();
            response = HandleRequestWrapper<AccountReadAccountResponse>(request, res =>
            {
                var accountManager = mm.GetManager<IAccountManager>();
                if (!origin.HasData<AccountData>() || !origin.GetData<AccountData>().Admin)
                {
                    if (request.AccountIds == null || request.AccountIds.Count() > 1 ||
                        request.AccountIds.First() != origin.GetData<AccountData>().Id)
                    {
                        throw new BotAuthorizationDeniedException("Permission denied");
                    }
                }
                var accountDatas = (request.AccountIds == null) ?
                    accountManager.GetRepository<IAccountDataRepo>().Read() :
                    accountManager.GetRepository<IAccountDataRepo>().Read(request.AccountIds);
                ((AccountReadAccountResponse)res).Accounts = accountDatas;
                res.SetSuccess(true);
            });
            return response;
        }
    }
}
