using BeforeOurTime.Business.Dbs.EF;
using BeforeOurTime.Models;
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
        /// Table that repository will operate with
        /// </summary>
        protected DbSet<Item> Set { set; get; }
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
        public ItemRepo(EFCoreModuleContext db) : base(db) { 
            Set = db.GetDbSet<Item>();
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
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>List of items created</returns>
        override public List<Item> Create(List<Item> items, TransactionOptions options = null)
        {
            Set.AddRange(items);
            Db.SaveChanges();
            if (OnItemCreate != null)
            {
                void InvokeOnItemCreate(List<Item> invokeItems)
                {
                    invokeItems.ForEach(item =>
                    {
                        OnItemCreate(item, options);
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
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>List of items</returns>
        override public List<Item> Read(List<Guid> ids, TransactionOptions options = null)
        {
            options = options ?? new TransactionOptions()
            {
                NoTracking = true
            };
            var resultSet = Set
                .Where(x => ids.Contains(x.Id))
                .Include(x => x.Parent)
                .Include(x => x.Children)
                .AsQueryable();
            resultSet = (options?.NoTracking == true) ? resultSet.AsNoTracking() : resultSet;
            List<Item> items = resultSet.ToList();
            if (OnItemRead != null)
            {
                items.ForEach(delegate (Item item)
                {
                    // If this is the first time item is being loaded then allow alterations
                    if (item.Attributes.Count == 0)
                    {
                        OnItemRead(item, options);
                    }
                    item.Children?.ForEach((child) =>
                    {
                        if (child.Attributes.Count == 0)
                        {
                            OnItemRead(child, options);
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
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>List of items updated</returns>
        override public List<Item> Update(List<Item> items, TransactionOptions options = null)
        {
            base.Update(items, options);
            // Create child items that are new
            void ItemCreate(List<Item> createItems)
            {
                Create(createItems, options);
                createItems.ForEach(item =>
                {
                    ItemCreate(item.Children?.Where(x => x.Id == null).ToList());
                });
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
                        OnItemUpdate(item, options);
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
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>List of items created</returns>
        override public void Delete(List<Item> items, TransactionOptions options = null)
        {
            if (OnItemDelete != null)
            {
                items.ForEach(delegate (Item item)
                {
                    OnItemDelete(item, options);
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
            return Set.Where(x => x.ParentId != null && x.ParentId == itemId).Select(x => x.Id).ToList();
        }
    }
}
