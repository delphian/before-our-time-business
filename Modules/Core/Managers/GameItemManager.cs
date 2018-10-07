using BeforeOurTime.Models.Items;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Models.Modules.Core.Dbs;
using BeforeOurTime.Models.Modules.Core.Models.Data;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using Microsoft.Extensions.Logging;
using BeforeOurTime.Models.Logs;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Modules.Core.Managers;

namespace BeforeOurTime.Business.Modules.Core.Managers
{
    public class GameItemManager : ItemModelManager<GameItem>, IGameItemManager
    {
        /// <summary>
        /// Centralized log messages
        /// </summary>
        private IBotLogger Logger { set; get; }
        private IGameDataRepo GameDataRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public GameItemManager(
            IBotLogger logger,
            IItemRepo itemRepo,
            IGameDataRepo gameDataRepo) : base(itemRepo)
        {
            Logger = logger;
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
    }
}
