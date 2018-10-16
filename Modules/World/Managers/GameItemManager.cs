using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using BeforeOurTime.Models.Logs;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.World.Models.Items;
using BeforeOurTime.Models.Modules.World.Managers;
using BeforeOurTime.Models.Modules.World.Dbs;
using BeforeOurTime.Models.Modules.World.Models.Data;
using BeforeOurTime.Models.Modules.Core.Models.Items;

namespace BeforeOurTime.Business.Modules.World.Managers
{
    public class GameItemManager : ItemModelManager<GameItem>, IGameItemManager
    {
        /// <summary>
        /// Manage all modules
        /// </summary>
        private IModuleManager ModuleManager { set; get; }
        /// <summary>
        /// Repository for manager
        /// </summary>
        private IGameDataRepo GameDataRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public GameItemManager(
            IModuleManager moduleManager,
            IGameDataRepo gameDataRepo)
        {
            ModuleManager = moduleManager;
            GameDataRepo = gameDataRepo;
        }
        /// <summary>
        /// Update games's default location
        /// </summary>
        /// <param name="id">Unique game attribute identifier</param>
        /// <param name="locationId">Game's new default location</param>
        /// <returns></returns>
        public GameData UpdateDefaultLocation(Guid id, Guid locationId)
        {
            var gameData = GameDataRepo.Read(id);
            gameData.DefaultLocationId = locationId;
            return GameDataRepo.Update(gameData);
        }
        /// <summary>
        /// Get all repositories declared by manager
        /// </summary>
        /// <returns></returns>
        public List<ICrudModelRepository> GetRepositories()
        {
            return new List<ICrudModelRepository>() { GameDataRepo };
        }
        /// <summary>
        /// Get repository as interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetRepository<T>() where T : ICrudModelRepository
        {
            return GetRepositories().Where(x => x is T).Select(x => (T)x).FirstOrDefault();
        }
        /// <summary>
        /// Get all unique item identifiers of managed items
        /// </summary>
        /// <returns></returns>
        public List<Guid> GetItemIds()
        {
            var itemIds = GameDataRepo.GetItemIds();
            return itemIds;
        }
        /// <summary>
        /// Determine if item data type is managable
        /// </summary>
        /// <param name="propertyData">Item data type that might be managable</param>
        public bool IsManagingData(Type dataType)
        {
            return dataType == typeof(GameData);
        }
        #region On Item Hooks
        /// <summary>
        /// Create attribute, if present, after item is created
        /// </summary>
        /// <param name="item">Base item just created from datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemCreate(Item item, TransactionOptions options = null)
        {
            if (item.HasData<GameData>())
            {
                var data = item.GetData<GameData>();
                data.DataItemId = item.Id;
                GameDataRepo.Create(data, options);
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item just read from datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemRead(Item item, TransactionOptions options = null)
        {
            var gameData = GameDataRepo.Read(item, options);
            if (gameData != null)
            {
                item.Data.Add(gameData);
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item about to be persisted to datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemUpdate(Item item, TransactionOptions options = null)
        {
            if (item.HasData<GameData>())
            {
                var data = item.GetData<GameData>();
                GameDataRepo.Update(data, options);
            }
        }
        /// <summary>
        /// Delete attribute of base item before base item is deleted
        /// </summary>
        /// <param name="item">Base item about to be deleted</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemDelete(Item item, TransactionOptions options = null)
        {
            if (item.HasData<GameData>())
            {
                var data = item.GetData<GameData>();
                GameDataRepo.Delete(data, options);
            }
        }
        #endregion
    }
}
