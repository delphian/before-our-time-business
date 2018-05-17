using BeforeOurTime.Business.Apis.Messages;
using BeforeOurTime.Business.Apis.Scripts;
using BeforeOurTime.Repository.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Games
{
    class ItemCharacterManager : ItemManager, IItemCharacterManager
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="itemRepo"></param>
        /// <param name="messageManager"></param>
        /// <param name="scriptManager"></param>
        public ItemCharacterManager(
            IItemRepo<Item> itemRepo,
            IMessageManager messageManager,
            IScriptManager scriptManager) : base(itemRepo, messageManager, scriptManager)
        {
        }
        /// <summary>
        /// Create a new character
        /// </summary>
        /// <param name="name">Public name of the character</param>
        /// <param name="accountId">Account to which this character belongs</param>
        /// <param name="parentId">Location of new item</param>
        public ItemCharacter Create(
            string name,
            Guid accountId,
            Guid parentId)
        {
            var parent = Read<Item>(parentId);
            var game = Read<Item>(new Guid("487a7282-0cad-4081-be92-83b14671fc23"));
            var character = new ItemCharacter()
            {
                Name = name,
                AccountId = accountId,
                Type = ItemType.Character,
                UuidType = Guid.NewGuid(),
                ParentId = parentId,
                Data = "{}",
                Script = "{ function onTick(e) {}; function onTerminalOutput(e) { terminalMessage(e.terminal.id, e.raw); }; function onItemMove(e) { }; }"
            };
            Create<ItemCharacter>(game, character);
            return character;
        }
    }
}
