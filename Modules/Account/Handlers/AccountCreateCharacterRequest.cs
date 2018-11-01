using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Account.Managers;
using BeforeOurTime.Models.Modules.Account.Messages.CreateCharacter;
using BeforeOurTime.Models.Modules.Terminal.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Modules.Account.Managers
{
    public partial class AccountCharacterManager
    {
        /// <summary>
        /// Handle a message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="mm">Module manager</param>
        /// <param name="terminal">Terminal that initiated request</param>
        /// <param name="response"></param>
        public IResponse HandleCreateCharacterRequest(
            IMessage message,
            IModuleManager mm,
            ITerminal terminal,
            IResponse response)
        {
            var request = message.GetMessageAsType<AccountCreateCharacterRequest>();
            response = HandleRequestWrapper<AccountCreateCharacterResponse>(request, res =>
            {
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
