﻿using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Business.Apis.Scripts;
using System.Linq;
using BeforeOurTime.Business.Apis.Messages;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Scripts.Delegates;
using BeforeOurTime.Models;

namespace BeforeOurTime.Business.Apis.Items
{
    /// <summary>
    /// Manages security, environment messages, CRUD, and run time considerations for items
    /// </summary>
    public class ItemManager : IItemManager
    {
        private IItemRepo ItemRepo { set; get; }
        private IMessageManager MessageManager { set; get; }
        private IScriptManager ScriptManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="itemRepo"></param>
        /// <param name="scriptManager"></param>
        /// <param name="messageManager"></param>
        public ItemManager(
            IItemRepo itemRepo,
            IMessageManager messageManager,
            IScriptManager scriptManager)
        {
            ItemRepo = itemRepo;
            MessageManager = messageManager;
            ScriptManager = scriptManager;
        }
        /// <summary>
        /// Create a new item
        /// </summary>
        /// <param name="item">Item which is new and being created</param>
        public Item Create(Item item)
        {
            item.DelegateLinks = UpdateScriptDelegateLinks(item);
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
        /// Get all item ids that implement a script delegate
        /// </summary>
        /// <param name="scriptDelegate">A script function name, it's argument type, and return type</param>
        /// <returns></returns>
        public List<Guid> GetDelegateImplementerIds(IDelegate scriptDelegate)
        {
            return ItemRepo.GetDelegateImplementerIds(scriptDelegate);
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
            item.DelegateLinks = UpdateScriptDelegateLinks(item);
            return Update(new List<Item>() { item }).FirstOrDefault();
        }
        /// <summary>
        /// Generate delegate links by parsing an item's script delegate declarations
        /// </summary>
        /// <param name="item">Item to generate delegate links for</param>
        /// <returns></returns>
        protected List<ScriptDelegateItemLink> UpdateScriptDelegateLinks(Item item)
        {
            var delegateLinks = new List<ScriptDelegateItemLink>();
            var invalidDelegates = ScriptManager.GetScriptInvalidDelegates(item.Script);
            if (invalidDelegates.Count > 0)
            {
                var delegateStrs = "";
                invalidDelegates.ForEach(delegate (IDelegate invalidDelegate)
                {
                    delegateStrs += (delegateStrs.Length == 0) ? invalidDelegate.GetFunctionName() :
                                                               ", " + invalidDelegate.GetFunctionName();
                });
                throw new Exception("Improperly declared script delegates: " + delegateStrs);
            }
            ScriptManager.GetScriptValidDelegates(item.Script).ForEach(delegate (IDelegate scriptDelegate)
            {
                delegateLinks.Add(new ScriptDelegateItemLink()
                {
                    Item = item,
                    DelegateId = scriptDelegate.GetId()
                });
            });
           return delegateLinks;
        }
        /// <summary>
        /// Update the item name
        /// </summary>
        /// <param name="id">Unique item identifier</param>
        /// <param name="name">New name of the item</param>
        /// <returns></returns>
        public Item UpdateName(Guid id, string name)
        {
            var item = Read(id);
            item.Name = name;
            return Update(item);
        }
        /// <summary>
        /// Update the item description
        /// </summary>
        /// <param name="id">Unique item identifier</param>
        /// <param name="description">New description of the item</param>
        /// <returns></returns>
        public Item UpdateDescription(Guid id, string description)
        {
            var item = Read(id);
            item.Description = description;
            return Update(item);
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
            MessageManager.SendDepartureEvent(item, oldLocation, source.Id);
            // Update item's location
            item.Parent = newParent;
            item.ParentId = newParent.Id;
            ItemRepo.Update(item);
            // Send arrival message
            var newLocation = ItemRepo.Read(newParent.Id);
            MessageManager.SendArrivalEvent(item, newLocation, source.Id);
            return item;
        }
    }
}