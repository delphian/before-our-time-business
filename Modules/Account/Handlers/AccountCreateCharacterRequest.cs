using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules.Account.Messages.CreateCharacter;
using BeforeOurTime.Models.Modules.Core;
using BeforeOurTime.Models.Modules.Core.Models.Data;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Terminals;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Modules.Account
{
    public partial class AccountModule
    {
        /// <summary>
        /// Handle a message
        /// </summary>
        /// <param name="api"></param>
        /// <param name="message"></param>
        /// <param name="terminal"></param>
        /// <param name="response"></param>
        private IResponse HandleCreateCharacterRequest(IMessage message, IApi api, ITerminal terminal, IResponse response)
        {
            var request = message.GetMessageAsType<AccountCreateCharacterRequest>();
            response = HandleRequestWrapper<AccountCreateCharacterResponse>(request, res =>
            {
                var characterItem = api.GetItemManager().Create(new CharacterItem()
                {
                    ParentId = api.GetModuleManager().GetModule<ICoreModule>().GetDefaultLocation().Id,
                    Data = new List<IItemData>()
                    {
                        new CharacterData()
                        {
                            Name = request.Name,
                            Description = "A brave new player"
                        }
                    }
                });
                ((AccountCreateCharacterResponse)res).CreatedAccountCharacterEvent = new AccountCreateCharacterEvent()
                {
                    ItemId = characterItem.Id
                };
            });
            return response;
        }
    }
}
