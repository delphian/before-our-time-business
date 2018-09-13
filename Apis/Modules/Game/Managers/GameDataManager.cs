using BeforeOurTime.Models.Items;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Business.Apis.Modules.Games.Data;
using BeforeOurTime.Business.Apis.Modules.Game.Models;
using BeforeOurTime.Business.Apis.Modules.Game.Managers;
using BeforeOurTime.Business.Managers;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Games
{
    public class GameDataManager : DataManager<GameData>, IGameDataManager
    {
        private IGameDataRepo GameDataRepo { set; get; }
        private IItemManager ItemManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public GameDataManager(
            IItemRepo itemRepo,
            IGameDataRepo gameDataRepo,
            IItemManager itemManager) : base(itemRepo, gameDataRepo)
        {
            GameDataRepo = gameDataRepo;
            ItemManager = itemManager;
        }
        /// <summary>
        /// Determine if an item has attributes that may be managed
        /// </summary>
        /// <param name="item">Item that may posses attributes</param>
        public bool IsManaging(Item item)
        {
            var managed = false;
            if (GameDataRepo.Read(item) != null)
            {
                managed = true;
            }
            return managed;
        }
        /// <summary>
        /// Update games's default location
        /// </summary>
        /// <param name="id">Unique game attribute identifier</param>
        /// <param name="locationId">Game's new default location</param>
        /// <returns></returns>
        public GameData UpdateDefaultLocation(Guid id, Guid locationId)
        {
            var gameAttribute = Read(id);
            gameAttribute.DefaultLocationId = locationId;
            return Update(gameAttribute);
        }
    }
}
