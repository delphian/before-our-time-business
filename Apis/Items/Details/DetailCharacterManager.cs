﻿using BeforeOurTime.Business.Apis.Scripts;
using BeforeOurTime.Business.Apis.Scripts.Engines;
using BeforeOurTime.Business.Apis.Scripts.Libraries;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Details;
using BeforeOurTime.Repository.Models.Items.Details.Repos;
using BeforeOurTime.Repository.Models.Messages;
using Newtonsoft.Json;
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
        private IScriptEngine ScriptEngine { set; get; }
        private IScriptManager ScriptManager { set; get; }
        private IItemManager ItemManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public DetailCharacterManager(
            IItemRepo itemRepo,
            IDetailCharacterRepo detailCharacterRepo,
            IScriptEngine scriptEngine,
            IScriptManager scriptManager,
            IItemManager itemManager)
        {
            ItemRepo = itemRepo;
            DetailCharacterRepo = detailCharacterRepo;
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
            return ItemType.Character;
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
            var characterItem = ItemManager.Create(new Item()
            {
                Type = ItemType.Character,
                UuidType = Guid.NewGuid(),
                ParentId = parentId,
                Data = "{}",
                Script = "function onTick(e) {}; function onTerminalOutput(e) { terminalMessage(e.terminal.id, e.raw); }; function onItemMove(e) { };"
            });
            var character = DetailCharacterRepo.Create(new List<DetailCharacter>() { new DetailCharacter()
            {
                Item = characterItem,
                Name = name,
                AccountId = accountId,
            } }).FirstOrDefault();

            //            ItemManager.Materialize(character);

            return character;
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
    }
}
