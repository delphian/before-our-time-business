using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Account.Managers;
using BeforeOurTime.Models.Modules.Account.Messages.CreateCharacter;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Terminal.Managers;
using BeforeOurTime.Models.Modules.Terminal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Modules.Account.Managers
{
    public partial class AccountCharacterManager
    {
        /// <summary>
        /// Handle a message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="origin">Item that initiated request</param>
        /// <param name="mm">Module manager</param>
        /// <param name="response"></param>
        public IResponse HandleCreateCharacterRequest(
            IMessage message,
            Item origin,
            IModuleManager mm,
            IResponse response)
        {
            var request = message.GetMessageAsType<AccountCreateCharacterRequest>();
            response = HandleRequestWrapper<AccountCreateCharacterResponse>(request, res =>
            {
                var terminal = mm.GetManager<ITerminalManager>().GetTerminals().Where(x => x.GetId() == origin.TerminalId).FirstOrDefault();
                var characterItem = ModuleManager.GetManager<IAccountCharacterManager>()
                    .Create(terminal.GetAccountId().Value, request.Name, request.Temporary);
                ((AccountCreateCharacterResponse)res).CreatedAccountCharacterEvent = new AccountCreateCharacterEvent()
                {
                    ItemId = characterItem.Id
                };
                res.SetSuccess(true);
            });
            return response;
        }
    }
}
