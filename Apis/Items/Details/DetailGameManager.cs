using BeforeOurTime.Business.Apis.Messages;
using BeforeOurTime.Business.Apis.Scripts;
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
    public class DetailGameManager : IDetailGameManager
    {
        private IItemRepo ItemRepo { set; get; }
        private IDetailGameRepo DetailGameRepo { set; get; }
        private IDetailLocationRepo DetailLocationRepo { set; get; }
        private IScriptEngine ScriptEngine { set; get; }
        private IScriptManager ScriptManager { set; get; }
        private IItemManager ItemManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public DetailGameManager(
            IItemRepo itemRepo,
            IDetailGameRepo detailGameRepo,
            IDetailLocationRepo detailLocationRepo,
            IScriptEngine scriptEngine,
            IScriptManager scriptManager,
            IItemManager itemManager)
        {
            ItemRepo = itemRepo;
            DetailGameRepo = detailGameRepo;
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
            return ItemType.Game;
        }
        /// <summary>
        /// Create a game item
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public DetailGame Create(DetailGame game)
        {
            return DetailGameRepo.Create(game);
        }
        /// <summary>
        /// Get the default game
        /// </summary>
        /// <remarks>
        /// Will create the default game and a location if one does not already exist
        /// </remarks>
        /// <returns></returns>
        public DetailGame GetDefaultGame()
        {
            var defaultGame = DetailGameRepo.Read().FirstOrDefault();
            if (defaultGame == null)
            {
                defaultGame = Create(new DetailGame()
                {
                    Name = "Brave New World",
                    Item = ItemManager.Create(new Item()
                    {
                        Type = ItemType.Game,
                        UuidType = Guid.NewGuid(),
                    }),
                    DefaultLocation = new DetailLocation()
                    {
                        Name = "The Dark Void",
                        Description = "Eternal darkness reigns supreme in this place. Complete seperation from all that is good, holy, and pure are the only known virtues.",
                        Item = ItemManager.Create(new Item()
                        {
                            Type = ItemType.Location,
                            UuidType = Guid.NewGuid()
                        })
                    }
                });
            }
            return defaultGame;
        }
        /// <summary>
        /// Get the default location of the default game
        /// </summary>
        /// <remarks>
        /// Default locations are used as the parent for item's whos parent is
        /// not specified
        /// </remarks>
        /// <returns></returns>
        public DetailLocation GetDefaultLocation()
        {
            var defaultGame = GetDefaultGame();
            return DetailLocationRepo.Read(defaultGame.DefaultLocationId);
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
