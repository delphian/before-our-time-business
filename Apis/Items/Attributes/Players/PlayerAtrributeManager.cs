using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Business.Apis.Scripts;
using BeforeOurTime.Business.Apis.Scripts.Engines;
using BeforeOurTime.Business.Apis.Scripts.Libraries;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Items.Attributes.Players;
using BeforeOurTime.Models.Items.Attributes.Characters;
using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Messages.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Models.Items.Attributes;
using BeforeOurTime.Business.Apis.Items.Attributes.Physicals;
using BeforeOurTime.Business.Apis.Items.Attributes.Characters;
using BeforeOurTime.Models.Items.Attributes.Locations;
using BeforeOurTime.Models.Items.Attributes.Physicals;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Players
{
    public class PlayerAttributeManager : AttributeManager<PlayerAttribute>, IPlayerAttributeManager
    {
        private IScriptEngine ScriptEngine { set; get; }
        private IScriptManager ScriptManager { set; get; }
        private IItemManager ItemManager { set; get; }
        private IPhysicalAttributeManager AttributePhysicalManager { set; get; }
        private ICharacterAttributeManager CharacterAttributeManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public PlayerAttributeManager(
            IItemRepo itemRepo,
            IPlayerAttributeRepo playerAttributeRepo,
            IScriptEngine scriptEngine,
            IScriptManager scriptManager,
            IItemManager itemManager,
            IPhysicalAttributeManager attributePhysicalManager,
            ICharacterAttributeManager characterAttributeManager) : base(itemRepo, playerAttributeRepo)
        {
            ScriptEngine = scriptEngine;
            ScriptManager = scriptManager;
            ItemManager = itemManager;
            AttributePhysicalManager = attributePhysicalManager;
            CharacterAttributeManager = characterAttributeManager;
        }
        /// <summary>
        /// Create a new player
        /// </summary>
        /// <param name="characer">Properties generally understood to denote a state of being alive</param>
        /// <param name="physical">Physical attributes</param>
        /// <param name="player">Player attributes</param>
        /// <param name="initialLocation">Location of new player</param>
        public Item Create(
            CharacterAttribute character,
            PhysicalAttribute physical,
            PlayerAttribute player,
            LocationAttribute initialLocation)
        {
            // Create item
            var item = ItemManager.Create(new Item()
            {
                UuidType = Guid.NewGuid(),
                ParentId = initialLocation.ItemId,
                Data = "{}",
                Script = "",
                Attributes = new List<ItemAttribute>()
                {
                    physical,
                    player,
                    character
                }
            });
            return item;
        }
        /// <summary>
        /// Deliver a message to an item
        /// </summary>
        /// <remarks>
        /// Often results in the item's script executing and parsing the message package
        /// </remarks>
        /// <param name="item"></param>
        public void DeliverMessage(SavedMessage message, Item item, JsFunctionManager jsFunctionManager)
        {
            var functionDefinition = ScriptManager.GetDelegateDefinition(message.DelegateId);
            if (ScriptEngine.GetFunctionDeclarations(item.Script.Trim()).Contains(functionDefinition.GetFunctionName()))
            {
                jsFunctionManager.AddJsFunctions(ScriptEngine);
                ScriptEngine
                    .SetValue("me", item)
                    .SetValue("_data", JsonConvert.SerializeObject(JsonConvert.DeserializeObject(item.Data)))
                    .Execute("var data = JSON.parse(_data);")
                    .Execute(item.Script)
                    .Invoke(
                        functionDefinition.GetFunctionName(),
                        JsonConvert.DeserializeObject(message.Package, functionDefinition.GetArgumentType())
                    );
                // Save changes to item data
                item.Data = JsonConvert.SerializeObject(ScriptEngine.GetValue("data"));
                ItemManager.Update(item);
            }
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
