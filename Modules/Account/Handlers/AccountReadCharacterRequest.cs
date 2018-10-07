using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules.Account.Messages.ReadCharacter;
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
        private IResponse HandleReadCharacterRequest(IMessage message, IApi api, ITerminal terminal, IResponse response)
        {
            var request = message.GetMessageAsType<AccountReadCharacterRequest>();
            response = HandleRequestWrapper<AccountReadCharacterResponse>(request, res =>
            {
                //var itemIds = api.GetAttributeManager<IPlayerAttributeManager>()
                //    .Read()
                //    .Where(x => x.AccountId == listAccountCharactersRequest.AccountId)
                //    .Select(x => x.ItemId)
                //    .ToList();
                //var items = api.GetItemManager()
                //    .Read(itemIds);
                //var characterItems = items
                //    .Select(x => x.GetAsItem<CharacterItem>())
                //    .ToList();
                //var listAccountCharactersResponse = new AccountReadCharacterResponse()
                //{
                //    _requestInstanceId = request.GetRequestInstanceId(),
                //    _responseSuccess = true,
                //    AccountCharacters = characterItems
                //};
                res.SetSuccess(false);
            });
            return response;
        }
    }
}
