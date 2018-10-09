﻿using BeforeOurTime.Models.Items;
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
using BeforeOurTime.Models.Modules.Core.Models.Data;
using BeforeOurTime.Business.Apis.Items;
using BeforeOurTime.Models.Modules;

namespace BeforeOurTime.Business.Modules.Core.Managers
{
    public class CharacterItemManager : ItemModelManager<CharacterItem>, ICharacterItemManager
    {
        /// <summary>
        /// Manage all modules
        /// </summary>
        private IModuleManager ModuleManager { set; get; }
        /// <summary>
        /// Repository for manager
        /// </summary>
        private ICharacterDataRepo CharacterDataRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public CharacterItemManager(
            IModuleManager moduleManager,
            ICharacterDataRepo characterDataRepo)
        {
            ModuleManager = moduleManager;
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
        /// <summary>
        /// Create a new character
        /// </summary>
        /// <param name="name">Name of character</param>
        /// <param name="locationItemId">Initial location of new character</param>
        /// <returns></returns>
        public CharacterItem Create(string name, Guid locationItemId)
        {
            var item = ModuleManager.GetItemRepo().Create(new CharacterItem()
            {
                ParentId = locationItemId,
                Data = new List<IItemData>()
                {
                    CharacterDataRepo.Create(new CharacterData()
                    {
                        Name = name,
                        Description = "A brave new player"
                    })
                }
            });
            return item.GetAsItem<CharacterItem>();
        }
        #region On Item Hooks
        /// <summary>
        /// Create attribute, if present, after item is created
        /// </summary>
        /// <param name="item">Base item just created from datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemCreate(Item item, TransactionOptions options = null)
        {
            if (item.HasData<CharacterData>())
            {
                var data = item.GetData<CharacterData>();
                data.DataItemId = item.Id;
                CharacterDataRepo.Create(data, options);
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item just read from datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemRead(Item item, TransactionOptions options = null)
        {
            var characterData = CharacterDataRepo.Read(item, options);
            if (characterData != null)
            {
                item.Data.Add(characterData);
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item about to be persisted to datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemUpdate(Item item, TransactionOptions options = null)
        {
            if (item.HasData<CharacterData>())
            {
                var data = item.GetData<CharacterData>();
                CharacterDataRepo.Update(data, options);
            }
        }
        /// <summary>
        /// Delete attribute of base item before base item is deleted
        /// </summary>
        /// <param name="item">Base item about to be deleted</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemDelete(Item item, TransactionOptions options = null)
        {
            if (item.HasData<CharacterData>())
            {
                var data = item.GetData<CharacterData>();
                CharacterDataRepo.Delete(data, options);
            }
        }
        #endregion
    }
}