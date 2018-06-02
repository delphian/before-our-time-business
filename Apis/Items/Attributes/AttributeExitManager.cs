﻿using BeforeOurTime.Business.Apis.Items.Attributes.Interfaces;
using BeforeOurTime.Business.Apis.Scripts;
using BeforeOurTime.Business.Apis.Scripts.Engines;
using BeforeOurTime.Business.Apis.Scripts.Libraries;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Attributes;
using BeforeOurTime.Repository.Models.Items.Attributes.Exits;
using BeforeOurTime.Repository.Models.Items.Attributes.Repos;
using BeforeOurTime.Repository.Models.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes
{
    public class AttributeExitManager : AttributeManager<AttributeExit>, IAttributeExitManager
    {
        private IItemRepo ItemRepo { set; get; }
        private IScriptEngine ScriptEngine { set; get; }
        private IScriptManager ScriptManager { set; get; }
        private IItemManager ItemManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public AttributeExitManager(
            IItemRepo itemRepo,
            IAttributeExitRepo attributeExitRepo,
            IScriptEngine scriptEngine,
            IScriptManager scriptManager,
            IItemManager itemManager) : base(attributeExitRepo)
        {
            ItemRepo = itemRepo;
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
            return ItemType.Generic;
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
        /// Determine if an item has attributes that may be managed
        /// </summary>
        /// <param name="item">Item that may posses attributes</param>
        public bool IsManaging(Item item)
        {
            var managed = false;
            if (AttributeRepo.Read(item) != null) {
                managed = true;
            }
            return managed;
        }
        /// <summary>
        /// Update the attribute name
        /// </summary>
        /// <param name="id">Unique attribute identifier</param>
        /// <param name="name">New name of the attribute</param>
        /// <returns></returns>
        public AttributeExit UpdateName(Guid id, string name)
        {
            var attribute = Read(id);
            attribute.Name = name;
            return Update(attribute);
        }
        /// <summary>
        /// Update the attribute description
        /// </summary>
        /// <param name="id">Unique attribute identifier</param>
        /// <param name="description">New description of the attribute</param>
        /// <returns></returns>
        public AttributeExit UpdateDescription(Guid id, string description)
        {
            var attribute = Read(id);
            attribute.Description = description;
            return Update(attribute);
        }
        /// <summary>
        /// Update the destination location
        /// </summary>
        /// <param name="id">Unique exit attribute identifier</param>
        /// <param name="destinationLocationId">New location id of the exit destination</param>
        /// <returns></returns>
        public AttributeExit UpdateDestination(Guid id, Guid destinationLocationId)
        {
            var exitAttribute = Read(id);
            exitAttribute.DestinationLocationId = destinationLocationId;
            return Update(exitAttribute);
        }
    }
}