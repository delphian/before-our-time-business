using BeforeOurTime.Business.Apis.Items.Attributes.Interfaces;
using BeforeOurTime.Business.Apis.Scripts;
using BeforeOurTime.Business.Apis.Scripts.Engines;
using BeforeOurTime.Business.Apis.Scripts.Libraries;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Items.Attributes;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Attributes;
using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Messages.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes
{
    public class AttributePlayerManager : AttributeManager<AttributePlayer>, IAttributePlayerManager
    {
        private IItemRepo ItemRepo { set; get; }
        private IScriptEngine ScriptEngine { set; get; }
        private IScriptManager ScriptManager { set; get; }
        private IItemManager ItemManager { set; get; }
        private IAttributePhysicalManager AttributePhysicalManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public AttributePlayerManager(
            IItemRepo itemRepo,
            IAttributePlayerRepo attributePlayerRepo,
            IScriptEngine scriptEngine,
            IScriptManager scriptManager,
            IItemManager itemManager,
            IAttributePhysicalManager attributePhysicalManager) : base(attributePlayerRepo)
        {
            ItemRepo = itemRepo;
            ScriptEngine = scriptEngine;
            ScriptManager = scriptManager;
            ItemManager = itemManager;
            AttributePhysicalManager = attributePhysicalManager;
        }
        /// <summary>
        /// Create a new player
        /// </summary>
        /// <param name="name">Public name of the player</param>
        /// <param name="accountId">Account to which this player belongs</param>
        /// <param name="physical">Physical attributes</param>
        /// <param name="initialLocation">Location of new player</param>
        public AttributePlayer Create(
            string name,
            Guid accountId,
            AttributePhysical physical,
            AttributeLocation initialLocation)
        {
            // Create item
            var item = ItemManager.Create(new Item()
            {
                UuidType = Guid.NewGuid(),
                ParentId = initialLocation.ItemId,
                Data = "{}",
                Script = "function onTick(e) {}; function onTerminalOutput(e) { terminalMessage(e.terminal.id, e.raw); }; function onItemMove(e) { };"
            });
            // Create player attributes
            var player = new AttributePlayer()
            {
                Name = name,
                AccountId = accountId,
            };
            // Attach all attributes
            Attach(player, item);
            AttributePhysicalManager.Attach(physical, item);
            return player;
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
