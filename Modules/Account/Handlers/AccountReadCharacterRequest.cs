using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Account.Messages.ReadCharacter;
using BeforeOurTime.Models.Modules.Terminal.Models;
using BeforeOurTime.Models.Modules.World.Models.Items;
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
        /// <param name="mm">Module manager</param>
        /// <param name="terminal">Terminal that initiated request</param>
        /// <param name="response"></param>
        public IResponse HandleReadCharacterRequest(
            IMessage message,
            IModuleManager mm,
            ITerminal terminal,
            IResponse response)
        {
            var request = message.GetMessageAsType<AccountReadCharacterRequest>();
            response = HandleRequestWrapper<AccountReadCharacterResponse>(request, res =>
            {
                var accountCharacters = ModuleManager.GetManager<AccountCharacterManager>()
                    .ReadByAccount(terminal.GetAccountId().Value);
                var items = ModuleManager.GetItemRepo()
                    .Read(accountCharacters.Select(x => x.CharacterItemId).ToList());
                ((AccountReadCharacterResponse)res).AccountCharacters = items.Select(x => x.GetAsItem<CharacterItem>()).ToList();
                res.SetSuccess(true);
            });
            return response;
        }
    }
}
