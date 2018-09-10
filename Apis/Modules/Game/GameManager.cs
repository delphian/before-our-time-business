using BeforeOurTime.Models;
using BeforeOurTime.Models.ItemAttributes.Games;
using BeforeOurTime.Models.Items;
using BeforeOutTime.Repository.Dbs.EF;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Modules.Game
{
    public class GameManager
    {
        /// <summary>
        /// Access to items in the data store
        /// </summary>
        private IItemRepo ItemRepo { set; get; }
        /// <summary>
        /// Access to Game Data in the data store
        /// </summary>
        private IGameAttributeRepo GameDataRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="itemRepo">Access to items in the data store</param>
        public GameManager(
            IItemRepo itemRepo,
            IGameAttributeRepo gameDataRepo)
        {
            ItemRepo = itemRepo;
            GameDataRepo = gameDataRepo;
            ItemRepo.OnItemCreate += OnItemCreate;
            ItemRepo.OnItemRead += OnItemRead;
            ItemRepo.OnItemUpdate += OnItemUpdate;
            ItemRepo.OnItemDelete += OnItemDelete;
        }
        /// <summary>
        /// Create attribute, if present, after item is created
        /// </summary>
        /// <param name="item">Base item just created from datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemCreate(Item item, TransactionOptions options = null)
        {
            if (item.HasAttribute<GameAttribute>())
            {
                var attribute = item.GetAttribute<GameAttribute>();
                attribute.ItemId = item.Id;
                GameDataRepo.Create(attribute, options);
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item just read from datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemRead(Item item, TransactionOptions options = null)
        {
            var attributes = GameDataRepo.Read(item, options);
            if (attributes != null)
            {
                item.Attributes.Add(attributes);
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item about to be persisted to datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemUpdate(Item item, TransactionOptions options = null)
        {
            if (item.HasAttribute<GameAttribute>())
            {
                var attribute = item.GetAttribute<GameAttribute>();
                GameDataRepo.Update(attribute, options);
            }
        }
        /// <summary>
        /// Delete attribute of base item before base item is deleted
        /// </summary>
        /// <param name="item">Base item about to be deleted</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemDelete(Item item, TransactionOptions options = null)
        {
            if (item.HasAttribute<GameAttribute>())
            {
                var attribute = item.GetAttribute<GameAttribute>();
                GameDataRepo.Delete(attribute, options);
            }
        }
    }
}
