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
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models.Modules.Core.Messages.MoveItem;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Modules.Core.Messages.ItemCrud.DeleteItem;
using BeforeOurTime.Models.Modules.Core.Messages.ItemCrud.CreateItem;

namespace BeforeOurTime.Business.Apis.Items
{
    /// <summary>
    /// Manages security, environment messages, CRUD, and run time considerations for items
    /// </summary>
    public class ItemManager : ModelManager<Item>, IItemManager
    {
        /// <summary>
        /// Subscribe to receive updates when an item changes locations
        /// </summary>
        public event ItemEventDelegate ItemMoveEvent;
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
            var createdItem = Read(item.Id);
            var messageManager = ModuleManager.GetManager<IMessageManager>();
            var parent = Read(createdItem.ParentId.Value);
            var recipients = new List<Item>();
            recipients.AddRange(parent.Children);
            recipients.Add(parent);
            var createItemEvent = new CoreCreateItemCrudEvent()
            {
                Item = createdItem
            };
            messageManager.SendMessage(new List<IMessage>() { createItemEvent }, recipients);
            return createdItem;
        }
        /// <summary>
        /// Read multiple models derived from Item
        /// </summary>
        /// <param name="itemIds">List of unique item identifiers</param>
        /// <returns></returns>
        public List<Item> Read(List<Guid> itemIds)
        {
            return ItemRepo.Read(itemIds);
        }
        /// <summary>
        /// Read single model of a type derived from Item
        /// </summary>
        /// <param name="itemIds">Unique item identifier</param>
        /// <returns></returns>
        public Item Read(Guid itemId)
        {
            return Read(new List<Guid>() { itemId }).FirstOrDefault();
        }
        /// <summary>
        /// Read all models derived from Item, or specify an offset and limit
        /// </summary>
        /// <param name="offset">Number of model records to skip</param>
        /// <param name="limit">Maximum number of model records to return</param>
        /// <returns></returns>
        public List<Item> Read(int? offset = null, int? limit = null)
        {
            return ItemRepo.Read(offset, limit);
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
            var messageManager = ModuleManager.GetManager<IMessageManager>();
            // Move item children
            items.ForEach((item) =>
            {
                if (deleteChildren == false)
                {
                    var oldParent = item.Parent;
                    if (oldParent != null)
                    {
                        item.Children?.ForEach((child) =>
                        {
                            Move(child, oldParent, item);
                        });
                    }
                    // Blank out the children, otherwise ef will try and delete 
                    item.Children = new List<Item>();
                }
                var deleteItemEvent = new CoreDeleteItemCrudEvent()
                {
                    Items = new List<Item>() { item }
                };
                var parentItem = Read(item.ParentId.Value);
                messageManager.SendMessage(new List<IMessage>() { deleteItemEvent },
                    new List<Item>() { parentItem });
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
            var oldLocation = ItemRepo.Read(item.ParentId.Value);
            // Update item's location
            item.Parent = newParent;
            item.ParentId = newParent.Id;
            ItemRepo.Update(item);
            var newLocation = ItemRepo.Read(newParent.Id);
            // Send messages
            var moveItemEvent = new CoreMoveItemEvent()
            {
                Item = ItemRepo.Read(item.Id),
                OldParent = oldLocation,
                NewParent = newParent,
                Source = source
            };
            var messageManager = ModuleManager.GetManager<IMessageManager>();
            var recipients = new List<Item>();
            var addRecipients = new List<Item>();
            addRecipients.AddRange(oldLocation.Children);
            addRecipients.AddRange(newLocation.Children);
            addRecipients.Add(oldLocation);
            addRecipients.Add(newLocation);
            addRecipients.ForEach(recipient =>
            {
                if (!recipients.Select(x => x.Id).ToList().Contains(recipient.Id))
                {
                    recipients.Add(recipient);
                }
            });
            ItemMoveEvent?.Invoke(moveItemEvent);
            messageManager.SendMessage(new List<IMessage>() { moveItemEvent }, recipients);
            return item;
        }
    }
}