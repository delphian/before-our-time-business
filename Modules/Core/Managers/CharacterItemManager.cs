using BeforeOurTime.Models.Items;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Models.Modules.Core.Dbs;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using System.Linq;
using Microsoft.Extensions.Logging;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models;

namespace BeforeOurTime.Business.Modules.Core.Managers
{
    public class CharacterItemManager : ItemModelManager<CharacterItem>, ICharacterItemManager
    {
        /// <summary>
        /// Centralized log messages
        /// </summary>
        private ILogger Logger { set; get; }
        private ICharacterDataRepo CharacterDataRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public CharacterItemManager(
            ILogger logger,
            IItemRepo itemRepo,
            ICharacterDataRepo characterDataRepo) : base(itemRepo)
        {
            Logger = logger;
            CharacterDataRepo = characterDataRepo;
        }
        /// <summary>
        /// Get all repositories declared by manager
        /// </summary>
        /// <returns></returns>
        public List<ICrudModelRepository> GetRepositories()
        {
            return new List<ICrudModelRepository>() { CharacterDataRepo };
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