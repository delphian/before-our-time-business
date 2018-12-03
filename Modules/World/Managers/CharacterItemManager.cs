using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Logging;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Modules.Core.Models.Data;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.World.Models.Items;
using BeforeOurTime.Models.Modules.World.Managers;
using BeforeOurTime.Models.Modules.World.Dbs;
using BeforeOurTime.Models.Modules.World.Models.Data;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Core.Messages.UseItem;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules.Terminal.Models;
using BeforeOurTime.Models.Modules.World.Models.Properties;

namespace BeforeOurTime.Business.Modules.World.Managers
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
            return (item.HasData<CharacterData>());
        }
        /// <summary>
        /// Determine if item data type is managable
        /// </summary>
        /// <param name="propertyData">Item data type that might be managable</param>
        public bool IsManagingData(Type dataType)
        {
            return dataType == typeof(CharacterData);
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
        public void OnItemCreate(Item item)
        {
            if (item.HasData<CharacterData>())
            {
                var data = item.GetData<CharacterData>();
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
            var characterData = CharacterDataRepo.Read(item);
            if (characterData != null)
            {
                item.Data.Add(characterData);
                item.AddProperty(typeof(CharacterProperty), item.GetProperty<CharacterProperty>());
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item about to be persisted to datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemUpdate(Item item)
        {
            if (item.HasData<CharacterData>())
            {
                var data = item.GetData<CharacterData>();
                CharacterDataRepo.Update(data);
            }
        }
        /// <summary>
        /// Delete attribute of base item before base item is deleted
        /// </summary>
        /// <param name="item">Base item about to be deleted</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemDelete(Item item)
        {
            if (item.HasData<CharacterData>())
            {
                var data = item.GetData<CharacterData>();
                CharacterDataRepo.Delete(data);
            }
        }
        #endregion
    }
}