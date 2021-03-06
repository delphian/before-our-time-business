﻿using BeforeOurTime.Models.Exceptions;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Account.Messages.RegisterCharacter;
using BeforeOurTime.Models.Modules.Account.Models.Data;
using BeforeOurTime.Models.Modules.Core.ItemProperties.Visibles;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.World.ItemProperties.Characters;
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
        public IResponse HandleRegisterCharacterRequest(
            IMessage message,
            Item origin,
            IModuleManager mm,
            IResponse response)
        {
            var request = message.GetMessageAsType<AccountRegisterCharacterRequest>();
            response = HandleRequestWrapper<AccountRegisterCharacterResponse>(request, res =>
            {
                var itemManager = mm.GetManager<IItemManager>();
                var character = itemManager.Read(request.CharacterId);
                if (origin.GetData<AccountData>().Id != character.GetData<AccountData>().Id)
                {
                    throw new BotAuthorizationDeniedException("Authorization denied");
                }
                character.GetData<CharacterItemData>().Temporary = false;
                character.GetData<VisibleItemData>().Name = request.Name;
                itemManager.Update(character);
                ((AccountRegisterCharacterResponse)res).Item = character;
                res.SetSuccess(true);
            });
            return response;
        }
    }
}
