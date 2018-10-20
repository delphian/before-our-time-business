using BeforeOurTime.Business.Dbs.EF;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Exceptions;
using BeforeOurTime.Models.Modules.Core.Dbs;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Modules.Core.Dbs.EF
{
    /// <summary>
    /// Access to items in the data store
    /// </summary>
    public class ItemRepo : Repository<Item>, IItemRepo
    {
        /// <summary>
        /// Item attribute repositories may attach to this event to perform action after item is created
        /// </summary>
        /// <remarks>
        /// Actions may include altering item properties or creating item attributes
        /// </remarks>
        public event onItemCreate OnItemCreate;
        /// <summary>
        /// Item attribute repositories may attach to this event to perform action before item is returned
        /// </summary>
        /// <remarks>
        /// Actions may include altering item properties or adding an item attribute
        /// </remarks>
        public event onItemRead OnItemRead;
        /// <summary>
        /// Item attribute repositories may attach to this event to perform action before an item is updated
        /// </summary>
        /// <remarks>
        /// Actions may include saving their own attribute property values
        /// </remarks>
        public event onItemUpdate OnItemUpdate;
        /// <summary>
        /// Item attribute repositories may attach to this event to perform action before an item is deleted
        /// </summary>
        /// <remarks>
        /// Actions may include deleting attributes
        /// </remarks>
        public event onItemDelete OnItemDelete;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">Entity framework database context</param>
        public ItemRepo(EFCoreModuleContext db) : base(db, db.GetDbSet<Item>())
        { 
        }
        /// <summary>
        /// Create multiple items
        /// </summary>
        /// <remarks>
        /// All other forms of create should call this one. Provides an event
        /// (OnItemCreate) to be used by attribute repos to perform their own
        /// actions after an item is created (such as create and persist
        /// attribute data)
        /// </remarks>
        /// <param name="items">List of items to create</param>
        /// <returns>List of items created</returns>
        override public List<Item> Create(List<Item> items)
        {
            if (items == null)
            {
                throw new BotDatabaseException("Can not create null items");
            }
            base.Create(items);
            if (OnItemCreate != null)
            {
                void InvokeOnItemCreate(List<Item> invokeItems)
                {
                    invokeItems.ForEach(item =>
                    {
                        OnItemCreate(item);
                        if (item.Children != null && item.Children.Count > 0)
                        {
                            InvokeOnItemCreate(item.Children);
                        }
                    });
                };
                InvokeOnItemCreate(items);
            }
            return items;
        }
        /// <summary>
        /// Read multiple items
        /// </summary>
        /// <remarks>
        /// All other forms of read should call this one. ItemRepo provides an
        /// event (OnItemRead) to be used when an item is first loaded that 
        /// allows subscribers to alter an item's attributes or properties
        /// </remarks>
        /// <param name="ids">List of unique item identifiers</param>
        /// <returns>List of items</returns>
        override public List<Item> Read(List<Guid> ids)
        {
            IQueryable<Item> resultSet;
            if (ids?.Count == 0)
            {
                resultSet = Set.AsNoTracking();
            }
            else
            {
                resultSet = Set
                    .Where(x => ids.Contains(x.Id))
                    .Include(x => x.Parent)
                    .Include(x => x.Children)
                    .AsQueryable()
                    .AsNoTracking();
            }
            List<Item> items = resultSet.ToList();
            if (OnItemRead != null)
            {
                items.ForEach(delegate (Item item)
                {
                    // If this is the first time item is being loaded then allow alterations
                    if (item.Data.Count == 0)
                    {
                        OnItemRead(item);
                    }
                    item.Children?.ForEach((child) =>
                    {
                        if (child.Data.Count == 0)
                        {
                            OnItemRead(child);
                        }
                    });
                    item.ChildrenIds = item.Children?.Select(x => x.Id)?.ToList();
                });
            }
            return items;
        }
        /// <summary>
        /// Update multiple items
        /// </summary>
        /// <remarks>
        /// All other forms of update should call this one. ItemRepo provides an
        /// event (OnItemUpdate) to be used when an item is updated that 
        /// allows subscribers to save their own attribute properties
        /// </remarks>
        /// <param name="ids">List of unique item identifiers</param>
        /// <returns>List of items updated</returns>
        override public List<Item> Update(List<Item> items)
        {
            base.Update(items);
            // Create child items that are new
            void ItemCreate(List<Item> createItems)
            {
                if (createItems != null)
                {
                    Create(createItems);
                    createItems.ForEach(item =>
                    {
                        ItemCreate(item.Children?.Where(x => x.Id == null).ToList());
                    });
                }
            }
            items.ForEach(item =>
            {
                ItemCreate(item.Children?.Where(x => x.Id == Guid.Empty).ToList());
            });
            // Update item data for existing items
            if (OnItemUpdate != null)
            {
                void InvokeOnItemUpdate(List<Item> invokeItems)
                {
                    invokeItems.ForEach(item =>
                    {
                        OnItemUpdate(item);
                        if (item.Children != null && item.Children.Count > 0)
                        {
                            InvokeOnItemUpdate(item.Children);
                        }
                    });
                };
                InvokeOnItemUpdate(items);
            }
            return items;
        }
        /// <summary>
        /// Delete multiple items
        /// </summary>
        /// <remarks>
        /// All other forms of delete should call this one. Provides an event
        /// (OnItemDelete) to be used by attribute repos to perform their own
        /// actions before an item is deleted (such as delete attribute data)
        /// </remarks>
        /// <param name="items">List of items to delete</param>
        /// <returns>List of items created</returns>
        override public void Delete(List<Item> items)
        {
            if (OnItemDelete != null)
            {
                items.ForEach(delegate (Item item)
                {
                    OnItemDelete(item);
                });
            }
            base.Delete(items);
        }
        /// <summary>
        /// Get the item identifiers of all item's children
        /// </summary>
        /// <param name="itemId">Unique item identifier of potential parent</param>
        /// <returns></returns>
        public List<Guid> GetChildrenIds(Guid itemId)
        {
            return Set
                .Where(x => x.ParentId != null && x.ParentId == itemId)
                .Select(x => x.Id).ToList();
        }
    }
}
