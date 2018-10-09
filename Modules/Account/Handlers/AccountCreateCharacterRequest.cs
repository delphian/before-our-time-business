using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules.Account.Managers;
using BeforeOurTime.Models.Modules.Account.Messages.CreateCharacter;
using BeforeOurTime.Models.Modules.Core;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models.Modules.Core.Models.Data;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Terminals;
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
        /// <param name="api"></param>
        /// <param name="message"></param>
        /// <param name="terminal"></param>
        /// <param name="response"></param>
        public IResponse HandleCreateCharacterRequest(IMessage message, IApi api, ITerminal terminal, IResponse response)
        {
            var request = message.GetMessageAsType<AccountCreateCharacterRequest>();
            response = HandleRequestWrapper<AccountCreateCharacterResponse>(request, res =>
            {
                var characterItem = ModuleManager.GetManager<IAccountCharacterManager>()
                    .Create(terminal.GetAccountId().Value, request.Name);
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
