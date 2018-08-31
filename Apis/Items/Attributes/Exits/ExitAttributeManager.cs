﻿using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Business.Apis.Scripts;
using BeforeOurTime.Business.Apis.Scripts.Engines;
using BeforeOurTime.Business.Apis.Scripts.Libraries;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Items.Attributes;
using BeforeOurTime.Models.Items.Attributes.Exits;
using BeforeOurTime.Models.Items.Attributes.Locations;
using BeforeOurTime.Repository.Models.Messages.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Exits
{
    public class ExitAttributeManager : AttributeManager<ExitAttribute>, IExitAttributeManager
    {
        private IScriptEngine ScriptEngine { set; get; }
        private IScriptManager ScriptManager { set; get; }
        private IItemManager ItemManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public ExitAttributeManager(
            IItemRepo itemRepo,
            IExitAttributeRepo exitAttributeRepo,
            IScriptEngine scriptEngine,
            IScriptManager scriptManager,
            IItemManager itemManager) : base(itemRepo, exitAttributeRepo)
        {
            ScriptEngine = scriptEngine;
            ScriptManager = scriptManager;
            ItemManager = itemManager;
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
        public ExitAttribute UpdateName(Guid id, string name)
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
        public ExitAttribute UpdateDescription(Guid id, string description)
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
        public ExitAttribute UpdateDestination(Guid id, Guid destinationLocationId)
        {
            var exitAttribute = Read(id);
            exitAttribute.DestinationLocationId = destinationLocationId;
            return Update(exitAttribute);
        }
        /// <summary>
        /// Get all inbound and outbound exits that link to and from a location
        /// </summary>
        /// <param name="location">Location item</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns></returns>
        public List<Item> GetLocationExits(Item location, TransactionOptions options = null)
        {
            var items = location.Children.Where(x => x.HasAttribute<ExitAttribute>()).ToList() ??
                        new List<Item>();
            items.AddRange(GetLocationInboundExits(location, options));
            return items;
        }
        /// <summary>
        /// Get all inbound exits that link to a location
        /// </summary>
        /// <param name="location">Location item</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns></returns>
        public List<Item> GetLocationInboundExits(Item location, TransactionOptions options = null)
        {
            var locationAttribute = location.GetAttribute<LocationAttribute>();
            var exitAttributes = ((IExitAttributeRepo)AttributeRepo)
                .ReadWithDestination(locationAttribute, options);
            var exitItems = ItemManager.Read(exitAttributes.Select(x => x.ItemId).ToList(), options);
            return exitItems;
        }
    }
}
