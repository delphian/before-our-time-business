using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.ItemAttributes;
using BeforeOurTime.Models.ItemAttributes.Characters;
using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Messages.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Characters
{
    /// <summary>
    /// Properties generally understood to denote a state of being alive
    /// </summary>
    public class CharacterAttributeManager : AttributeManager<CharacterAttribute>, ICharacterAttributeManager
    {
        private IItemManager ItemManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public CharacterAttributeManager(
            IItemRepo itemRepo,
            ICharacterAttributeRepo characterAttributeRepo,
            IItemManager itemManager) : base(itemRepo, characterAttributeRepo)
        {
            ItemManager = itemManager;
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
