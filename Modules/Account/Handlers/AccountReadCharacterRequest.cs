using BeforeOurTime.Business.Modules.Core.Managers;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules.Account.Messages.ReadCharacter;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Terminals;
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
        /// <param name="api"></param>
        /// <param name="message"></param>
        /// <param name="terminal"></param>
        /// <param name="response"></param>
        public IResponse HandleReadCharacterRequest(IMessage message, IApi api, ITerminal terminal, IResponse response)
        {
            var request = message.GetMessageAsType<AccountReadCharacterRequest>();
            response = HandleRequestWrapper<AccountReadCharacterResponse>(request, res =>
            {
                var accountCharacters = api.GetModuleManager().GetManager<AccountCharacterManager>()
                    .ReadByAccound(terminal.GetAccountId().Value);
                var items = api.GetItemManager()
                    .Read(accountCharacters.Select(x => x.AccountId).ToList());
                ((AccountReadCharacterResponse)res).AccountCharacters = items.Select(x => x.GetAsItem<CharacterItem>()).ToList();
                res.SetSuccess(true);
            });
            return response;
        }
    }
}
