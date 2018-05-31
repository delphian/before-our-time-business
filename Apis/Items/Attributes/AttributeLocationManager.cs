using BeforeOurTime.Business.Apis.Items.Attributes.Interfaces;
using BeforeOurTime.Business.Apis.Messages;
using BeforeOurTime.Business.Apis.Scripts;
using BeforeOurTime.Business.Apis.Scripts.Engines;
using BeforeOurTime.Business.Apis.Scripts.Libraries;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Attributes;
using BeforeOurTime.Repository.Models.Items.Attributes.Repos;
using BeforeOurTime.Repository.Models.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes
{
    public class AttributeLocationManager : AttributeManager<AttributeLocation>, IAttributeLocationManager
    {
        private IItemRepo ItemRepo { set; get; }
        private IDetailLocationRepo DetailLocationRepo { set; get; }
        private IScriptEngine ScriptEngine { set; get; }
        private IScriptManager ScriptManager { set; get; }
        private IItemManager ItemManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public AttributeLocationManager(
            IItemRepo itemRepo,
            IDetailLocationRepo detailLocationRepo,
            IScriptEngine scriptEngine,
            IScriptManager scriptManager,
            IItemManager itemManager) : base(detailLocationRepo)
        {
            ItemRepo = itemRepo;
            DetailLocationRepo = detailLocationRepo;
            ScriptEngine = scriptEngine;
            ScriptManager = scriptManager;
            ItemManager = itemManager;
        }
        /// <summary>
        /// Get the item type that the manager is responsible for providing detail management for
        /// </summary>
        /// <returns></returns>
        public ItemType GetItemType()
        {
            return ItemType.Location;
        }
        /// <summary>
        /// Deliver a message to an item
        /// </summary>
        /// <remarks>
        /// Often results in the item's script executing and parsing the message package
        /// </remarks>
        /// <param name="item"></param>
        public void DeliverMessage(Message message, Item item, JsFunctionManager jsFunctionManager)
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
        /// Read item's detailed location
        /// </summary>
        /// <remarks>
        /// If item itself has no location detail then item's parents will be 
        /// traversed until one is found
        /// </remarks>
        /// <param name="item">Item that has attached detail location data</param>
        /// <returns>The Item's detailed location data. Null if none found</returns>
        new public AttributeLocation Read(Item item)
        {
            AttributeLocation location = null;
            Item traverseItem = item;
            while (traverseItem.ParentId != null && traverseItem.Type != ItemType.Location)
            {
                traverseItem = ItemRepo.Read(traverseItem.ParentId.Value);
            }
            if (traverseItem.Type == ItemType.Location)
            {
                location = DetailLocationRepo.Read(traverseItem);
            }
            return location;
        }
        /// <summary>
        /// Determine if an item has attributes that may be managed
        /// </summary>
        /// <param name="item">Item that may posses attributes</param>
        public bool IsManaging(Item item)
        {
            var managed = false;
            if (DetailLocationRepo.Read(item) != null)
            {
                managed = true;
            }
            return managed;
        }
        /// <summary>
        /// Update the location name
        /// </summary>
        /// <param name="id">Unique location attribute identifier</param>
        /// <param name="name">New name of the location</param>
        /// <returns></returns>
        public AttributeLocation UpdateName(Guid id, string name)
        {
            var locationAttribute = Read(id);
            locationAttribute.Name = name;
            return Update(locationAttribute);
        }
        /// <summary>
        /// Update the location description
        /// </summary>
        /// <param name="id">Unique location attribute identifier</param>
        /// <param name="description">New description of the location</param>
        /// <returns></returns>
        public AttributeLocation UpdateDescription(Guid id, string description)
        {
            var locationAttribute = Read(id);
            locationAttribute.Description = description;
            return Update(locationAttribute);
        }
    }
}
