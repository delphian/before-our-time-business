using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Dbs;
using BeforeOurTime.Models.Modules.Core.Models.Items;

namespace BeforeOurTime.Business.Apis.Items
{
    /// <summary>
    /// Manages security, environment messages, CRUD, and run time considerations for items
    /// </summary>
    public class ItemManager : ModelManager<Item>, IItemManager
    {
        /// <summary>
        /// Manage all modules
        /// </summary>
        private IModuleManager ModuleManager { set; get; }
        /// <summary>
        /// Repository for manager
        /// </summary>
        private IItemRepo ItemRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="itemRepo"></param>
        /// <param name="scriptManager"></param>
        /// <param name="messageManager"></param>
        public ItemManager(
            IModuleManager moduleManager,
            IItemRepo itemDataRepo)
        {
            ModuleManager = moduleManager;
            ItemRepo = itemDataRepo;
        }
        /// <summary>
        /// Get all repositories declared by manager
        /// </summary>
        /// <returns></returns>
        public List<ICrudModelRepository> GetRepositories()
        {
            return new List<ICrudModelRepository>() { ItemRepo };
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
        /// Create a new item
        /// </summary>
        /// <param name="item">Item which is new and being created</param>
        public Item Create(Item item)
        {
            ItemRepo.Create(new List<Item>() { item });
            return item;
        }
        /// <summary>
        /// Read multiple models derived from Item
        /// </summary>
        /// <param name="itemIds">List of unique item identifiers</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns></returns>
        public List<Item> Read(List<Guid> itemIds, TransactionOptions options = null)
        {
            return ItemRepo.Read(itemIds, options);
        }
        /// <summary>
        /// Read single model of a type derived from Item
        /// </summary>
        /// <param name="itemIds">Unique item identifier</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns></returns>
        public Item Read(Guid itemId, TransactionOptions options = null)
        {
            return Read(new List<Guid>() { itemId }, options).FirstOrDefault();
        }
        /// <summary>
        /// Read all models derived from Item, or specify an offset and limit
        /// </summary>
        /// <param name="offset">Number of model records to skip</param>
        /// <param name="limit">Maximum number of model records to return</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns></returns>
        public List<Item> Read(int? offset = null, int? limit = null, TransactionOptions options = null)
        {
            return ItemRepo.Read(offset, limit, options);
        }
        /// <summary>
        /// Get the item identifiers of all item's children
        /// </summary>
        /// <param name="itemId">Unique item identifier of potential parent</param>
        /// <returns></returns>
        public List<Guid> GetChildrenIds(Guid itemId)
        {
            return ItemRepo.GetChildrenIds(itemId);
        }
        /// <summary>
        /// Update multiple models derived from Item
        /// </summary>
        /// <param name="itemIds">List of unique item identifiers</param>
        /// <returns></returns>
        public List<Item> Update(List<Item> items)
        {
            return ItemRepo.Update(items);
        }
        /// <summary>
        /// Update any model that is derived from Item
        /// </summary>
        /// <param name="item">Item to be updated</param>
        /// <returns></returns>
        public Item Update(Item item)
        {
            return Update(new List<Item>() { item }).FirstOrDefault();
        }
        /// <summary>
        /// Permenantly delete an item and remove from data store
        /// </summary>
        /// <remarks>
        /// All children will be re-homed to the item parent unless otherwise specified
        /// </remarks>
        /// <param name="items">List of items to delete</param>
        /// <param name="deleteChildren">Also delete all children</param>
        public void Delete(List<Item> items, bool? deleteChildren = false)
        {
            // Move item children
            items.ForEach((item) =>
            {
                if (deleteChildren == true)
                {
                    if (item.Children?.Count > 0)
                    {
                        Delete(item.Children, deleteChildren);
                    }
                }
                else
                {
                    var oldParent = item.Parent;
                    if (oldParent != null)
                    {
                        item.Children?.ForEach((child) =>
                        {
                            Move(child, oldParent, item);
                        });
                    }
                }
            });
            ItemRepo.Delete(items);
        }
        /// <summary>
        /// Relocate an item
        /// </summary>
        /// <param name="item">Item to move</param>
        /// <param name="newParent">Item which will become the parent</param>
        /// <param name="source">Item responsible for doing the moving</param>
        public Item Move(Item item, Item newParent, Item source = null)
        {
            // Send departure message
            var oldLocation = ItemRepo.Read(item.ParentId.Value);
//            MessageManager.SendDepartureEvent(item, oldLocation, source.Id);
            // Update item's location
            item.Parent = newParent;
            item.ParentId = newParent.Id;
            ItemRepo.Update(item);
            // Send arrival message
            var newLocation = ItemRepo.Read(newParent.Id);
//            MessageManager.SendArrivalEvent(item, newLocation, source.Id);
            return item;
        }
    }
}