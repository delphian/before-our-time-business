using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Details;
using BeforeOurTime.Repository.Models.Items.Details.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Details
{
    public class DetailCharacterManager : IDetailCharacterManager
    {
        private IItemRepo ItemRepo { set; get; }
        private IDetailCharacterRepo DetailCharacterRepo { set; get; }
        private ItemManager ItemManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public DetailCharacterManager(
            IItemRepo itemRepo,
            IDetailCharacterRepo detailCharacterRepo,
            ItemManager itemManager)
        {
            ItemRepo = itemRepo;
            DetailCharacterRepo = detailCharacterRepo;
            ItemManager = itemManager;
        }
        /// <summary>
        /// Create a new character
        /// </summary>
        /// <param name="name">Public name of the character</param>
        /// <param name="accountId">Account to which this character belongs</param>
        /// <param name="parentId">Location of new item</param>
        public DetailCharacter Create(
            string name,
            Guid accountId,
            Guid parentId)
        {
            var characterItem = ItemRepo.Create(new List<Item>() { new Item()
            {
                Type = ItemType.Character,
                UuidType = Guid.NewGuid(),
                ParentId = parentId,
                Data = "{}",
                Script = "{ function onTick(e) {}; function onTerminalOutput(e) { terminalMessage(e.terminal.id, e.raw); }; function onItemMove(e) { }; }"
            } }).FirstOrDefault();
            var character = DetailCharacterRepo.Create(new List<DetailCharacter>() { new DetailCharacter()
            {
                Item = characterItem,
                Name = name,
                AccountId = accountId,
            } }).FirstOrDefault();

            //            ItemManager.Materialize(character);

            return character;
        }
    }
}
