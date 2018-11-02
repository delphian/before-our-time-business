using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Account.Messages.LoginAccount;
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
        public IResponse HandleLoginAccountRequest(
            IMessage message,
            Item origin,
            IModuleManager mm,
            IResponse response)
        {
            var request = message.GetMessageAsType<AccountLoginAccountRequest>();
            response = HandleRequestWrapper<AccountLoginAccountResponse>(request, res =>
            {
                var account = Authenticate(request.Email, request.Password);
		if (account != null) {
                    ((AccountLoginAccountResponse)res).Account = new BeforeOurTime.Models.Modules.Account.Models.Account()
                    {
                        Id = account.Id,
                        Name = account.Name,
                        Password = null,
                        Temporary = account.Temporary
                    };
                    res.SetSuccess(true);
                }
                else
                {
                    res.SetSuccess(false).SetMessage($"Login for '{request.Email}' denied");
                }
            });
            return response;
        }
    }
}
