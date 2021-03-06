﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Logging;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Modules.Core.Models.Data;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.World.ItemProperties.Characters;
using BeforeOurTime.Models.Modules.Core.ItemProperties.Visibles;

namespace BeforeOurTime.Business.Modules.ItemProperties.Characters
{
    public class CharacterItemDataManager : ItemModelManager<Item>, ICharacterItemDataManager
    {
        /// <summary>
        /// Manage all modules
        /// </summary>
        private IModuleManager ModuleManager { set; get; }
        /// <summary>
        /// Repository for manager
        /// </summary>
        private ICharacterItemDataRepo CharacterDataRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public CharacterItemDataManager(
            IModuleManager moduleManager,
            ICharacterItemDataRepo characterDataRepo)
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
        /// Get all unique item identifiers of managed items
        /// </summary>
        /// <returns></returns>
        public List<Guid> GetItemIds()
        {
            var itemIds = CharacterDataRepo.GetItemIds();
            return itemIds;
        }
        /// <summary>
        /// Determine if an item is managed
        /// </summary>
        /// <param name="item">Item that may have managable data</param>
        public bool IsManaging(Item item)
        {
            return item.HasData<CharacterItemData>();
        }
        /// <summary>
        /// Determine if item data type is managable
        /// </summary>
        /// <param name="propertyData">Item data type that might be managable</param>
        public bool IsManagingData(Type dataType)
        {
            return dataType == typeof(CharacterItemData);
        }
        /// <summary>
        /// Create a new character
        /// </summary>
        /// <param name="name">Name of character</param>
        /// <param name="locationItemId">Initial location of new character</param>
        /// <returns></returns>
        public Item Create(string name, Guid locationItemId)
        {
            var item = ModuleManager.GetItemRepo().Create(new Item()
            {
                ParentId = locationItemId,
                Data = new List<IItemData>()
                {
                    new CharacterItemData()
                    {
                        Temporary = false
                    },
                    new VisibleItemData()
                    {
                        Name = name,
                        Description = "A brave new player"
                    }
                }
            });
            return item;
        }
        #region On Item Hooks
        /// <summary>
        /// Create attribute, if present, after item is created
        /// </summary>
        /// <param name="item">Base item just created from datastore</param>
        public void OnItemCreate(Item item)
        {
            if (item.HasData<CharacterItemData>())
            {
                var data = item.GetData<CharacterItemData>();
                data.DataItemId = item.Id;
                CharacterDataRepo.Create(data);
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item just read from datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemRead(Item item)
        {
            var data = CharacterDataRepo.Read(item);
            if (data != null)
            {
                item.Data.Add(data);
                item.AddProperty(typeof(CharacterItemProperty), new CharacterItemProperty()
                {
                    Temporary = data.Temporary,
                });
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item about to be persisted to datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemUpdate(Item item)
        {
            if (item.HasData<CharacterItemData>())
            {
                var data = item.GetData<CharacterItemData>();
                if (data.Id == Guid.Empty)
                {
                    OnItemCreate(item);
                }
                else
                {
                    CharacterDataRepo.Update(data);
                }
            }
        }
        /// <summary>
        /// Delete attribute of base item before base item is deleted
        /// </summary>
        /// <param name="item">Base item about to be deleted</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemDelete(Item item)
        {
            if (item.HasData<CharacterItemData>())
            {
                var data = item.GetData<CharacterItemData>();
                CharacterDataRepo.Delete(data);
            }
        }
        #endregion
    }
}