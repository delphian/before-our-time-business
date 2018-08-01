using BeforeOurTime.Business.Apis.Items.Attributes.Interfaces;
using BeforeOurTime.Business.Apis.Scripts;
using BeforeOurTime.Business.Apis.Scripts.Engines;
using BeforeOurTime.Business.Apis.Scripts.Libraries;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Items.Attributes;
using BeforeOurTime.Models.Primitives.Images;
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
    public class AttributePhysicalManager : AttributeManager<AttributePhysical>, IAttributePhysicalManager
    {
        private IAttributePhysicalRepo DetailPhysicalRepo { set; get; }
        private IScriptEngine ScriptEngine { set; get; }
        private IScriptManager ScriptManager { set; get; }
        private IItemManager ItemManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public AttributePhysicalManager(
            IItemRepo itemRepo,
            IAttributePhysicalRepo detailPhysicalRepo,
            IScriptEngine scriptEngine,
            IScriptManager scriptManager,
            IItemManager itemManager) : base(itemRepo, detailPhysicalRepo)
        {
            DetailPhysicalRepo = detailPhysicalRepo;
            ScriptEngine = scriptEngine;
            ScriptManager = scriptManager;
            ItemManager = itemManager;
        }
        /// <summary>
        /// Attach new physical attributes to an existing item
        /// </summary>
        /// <param name="item">Existing item that has already been saved</param>
        /// <param name="name">One, two, or three word short description of item</param>
        /// <param name="description">A long description of the item. Include many sensory experiences</param>
        /// <param name="height">Height</param>
        /// <param name="weight">Weight</param>
        public AttributePhysical Attach(
            Item item,
            string name,
            string description,
            int height,
            int weight)
        {
            var physicalAttributes = new AttributePhysical()
            {
                Name = name,
                Description = description,
                Height = height,
                Weight = weight,
                Item = item
            };
            var physical = Attach(physicalAttributes, item);
            return physical;
        }
        /// <summary>
        /// Create new item with new physical attributes
        /// </summary>
        /// <param name="parent">Parent item</param>
        /// <param name="name">One, two, or three word short description of item</param>
        /// <param name="description">A long description of the item. Include many sensory experiences</param>
        /// <param name="volume">Volume</param>
        /// <param name="weight">Weight</param>
        public AttributePhysical Create(
            Item parent,
            string name,
            string description,
            int volume,
            int weight)
        {
            var item = ItemManager.Create(new Item()
            {
                UuidType = Guid.NewGuid(),
                ParentId = parent.Id,
                Data = "{}",
                Script = ""
            });
            var physical = Attach(item, name, description, volume, weight);
            return physical;
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
            if (DetailPhysicalRepo.Read(item) != null) {
                managed = true;
            }
            return managed;
        }
        /// <summary>
        /// Update the physical name
        /// </summary>
        /// <param name="id">Unique phsyical attribute identifier</param>
        /// <param name="description">New name of the physical</param>
        /// <returns></returns>
        public AttributePhysical UpdateName(Guid id, string name)
        {
            var physicalAttribute = Read(id);
            physicalAttribute.Name = name;
            return Update(physicalAttribute);
        }
        /// <summary>
        /// Update the physical description
        /// </summary>
        /// <param name="id">Unique phsyical attribute identifier</param>
        /// <param name="description">New description of the physical</param>
        /// <returns></returns>
        public AttributePhysical UpdateDescription(Guid id, string description)
        {
            var physicalAttribute = Read(id);
            physicalAttribute.Description = description;
            return Update(physicalAttribute);
        }
        /// <summary>
        /// Update the physical attribute's image icon
        /// </summary>
        /// <param name="id">Unique phsyical attribute identifier</param>
        /// <param name="image">Wrapper for image that allows meta data</param>
        /// <returns></returns>
        public AttributePhysical UpdateImageIcon(Guid id, Image image)
        {
            var physicalAttribute = Read(id);
            physicalAttribute.ImageIcon = image;
            return Update(physicalAttribute);
        }
        /// <summary>
        /// Update the physical volume
        /// </summary>
        /// <param name="id">Unique phsyical attribute identifier</param>
        /// <param name="volume">New height of the physical</param>
        /// <returns></returns>
        public AttributePhysical UpdateHeight(Guid id, int height)
        {
            var physicalAttribute = Read(id);
            physicalAttribute.Height = height;
            return Update(physicalAttribute);
        }
        /// <summary>
        /// Update the physical weight
        /// </summary>
        /// <param name="id">Unique phsyical attribute identifier</param>
        /// <param name="weight">New weight of the physical</param>
        /// <returns></returns>
        public AttributePhysical UpdateWeight(Guid id, int weight)
        {
            var physicalAttribute = Read(id);
            physicalAttribute.Weight = weight;
            return Update(physicalAttribute);
        }
    }
}
