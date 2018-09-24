using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.ItemAttributes.Players;
using BeforeOurTime.Models.ItemAttributes.Characters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Models.ItemAttributes;
using BeforeOurTime.Business.Apis.Items.Attributes.Physicals;
using BeforeOurTime.Business.Apis.Items.Attributes.Characters;
using BeforeOurTime.Models.ItemAttributes.Physicals;
using BeforeOurTime.Models.Items.Players;
using BeforeOurTime.Models.ItemAttributes.Visibles;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Players
{
    public class PlayerAttributeManager : AttributeManager<PlayerAttribute>, IPlayerAttributeManager
    {
        private IItemManager ItemManager { set; get; }
        private IPhysicalAttributeManager AttributePhysicalManager { set; get; }
        private ICharacterAttributeManager CharacterAttributeManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public PlayerAttributeManager(
            IItemRepo itemRepo,
            IPlayerAttributeRepo playerAttributeRepo,
            IItemManager itemManager,
            IPhysicalAttributeManager attributePhysicalManager,
            ICharacterAttributeManager characterAttributeManager) : base(itemRepo, playerAttributeRepo)
        {
            ItemManager = itemManager;
            AttributePhysicalManager = attributePhysicalManager;
            CharacterAttributeManager = characterAttributeManager;
        }
        /// <summary>
        /// Create a new player
        /// </summary>
        /// <param name="character">Attributes describing how an item looks or is percieved</param>
        /// <param name="characer">Properties generally understood to denote a state of being alive</param>
        /// <param name="physical">Physical attributes</param>
        /// <param name="player">Player attributes</param>
        /// <param name="initialLocation">Location of new player</param>
        public PlayerItem Create(
            VisibleAttribute visible,
            CharacterAttribute character,
            PhysicalAttribute physical,
            PlayerAttribute player,
            Guid defaultLocationId)
        {
            var item = ItemManager.Create(new Item()
            {
                ParentId = defaultLocationId,
                Attributes = new List<ItemAttribute>()
                {
                    visible,
                    physical,
                    player,
                    character
                }
            }).GetAsItem<PlayerItem>();
            return item;
        }
        /// <summary>
        /// Determine if an item has attributes that may be managed
        /// </summary>
        /// <param name="item">Item that may posses attributes</param>
        public bool IsManaging(Item item)
        {
            var managed = false;
            if (AttributeRepo.Read(item) != null)
            {
                managed = true;
            }
            return managed;
        }
    }
}
