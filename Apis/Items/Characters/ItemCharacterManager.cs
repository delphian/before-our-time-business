using BeforeOurTime.Business.Apis.Items.Games;
using BeforeOurTime.Business.Apis.Messages;
using BeforeOurTime.Business.Apis.Scripts;
using BeforeOurTime.Repository.Models;
using BeforeOurTime.Repository.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Characters
{
    public class ItemCharacterManager : IItemCharacterManager
    {
        private IItemRepo<Item> ItemRepo { set; get; }
        private IItemGameRepo ItemGameRepo { set; get; }
//        private ItemManager ItemManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public ItemCharacterManager(
            IItemRepo<Item> itemRepo,
            IItemGameRepo itemGameRepo
//            ItemManager itemManager
            )
        {
            ItemRepo = itemRepo;
            ItemGameRepo = itemGameRepo;
//            ItemManager = itemManager;
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
            var parent = ItemRepo.Read(parentId);
            var game = ItemGameRepo.Read(new Guid("487a7282-0cad-4081-be92-83b14671fc23"));
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
//            ItemManager.Create(game, character);
            return character;
        }
    }
}
