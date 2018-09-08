using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.ItemAttributes;
using BeforeOurTime.Repository.Dbs.EF;
using BeforeOurTime.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes
{
    /// <summary>
    /// Manage details of an item's extended attributes
    /// </summary>
    public class AttributeManager<T> : IAttributeManager<T> where T : ItemAttribute
    {
        protected IItemRepo ItemRepo { set; get; }
        protected IAttributeRepo<T> AttributeRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="itemRepo"></param>
        /// <param name="attributeRepo"></param>
        public AttributeManager(
            IItemRepo itemRepo,
            IAttributeRepo<T> attributeRepo)
        {
            ItemRepo = itemRepo;
            AttributeRepo = attributeRepo;
        }
        /// <summary>
        /// Create multiple attributes
        /// </summary>
        /// <param name="attributes">List of attributes to create</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>List of attributes created</returns>
        public List<T> Create(List<T> attributes, TransactionOptions options = null)
        {
            return AttributeRepo.Create(attributes, options);
        }
        /// <summary>
        /// Create multiple attributes with base items
        /// </summary>
        /// <param name="attributes">List of attributes to create</param>
        /// <param name="parentId">Create the base items as children of this parent item</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>List of items created</returns>
        public List<Item> Create(List<T> attributes, Guid parentId, TransactionOptions options = null)
        {
            attributes.ForEach((attribute) =>
            {
                attribute.ItemId = ItemRepo.Create(new Item()
                {
                    ParentId = parentId
                }).Id;
            });
            Create(attributes, options);
            return ItemRepo.Read(attributes.Select(x => x.ItemId).ToList());
        }
        /// <summary>
        /// Create single attribute
        /// </summary>
        /// <param name="attribute">Attribute to create</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>Attribute created</returns>
        public T Create(T attribute, TransactionOptions options = null)
        {
            return Create(new List<T>() { attribute }, options).FirstOrDefault();
        }
        /// <summary>
        /// Create single attribute with base item
        /// </summary>
        /// <param name="attribute">Attribute to create</param>
        /// <param name="parentId">Create the base item as a child of this parent item</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>Item created</returns>
        public Item Create(T attribute, Guid parentId, TransactionOptions options = null)
        {
            return Create(new List<T> { attribute }, parentId, options).FirstOrDefault();
        }
        /// <summary>
        /// Read multiple attributes
        /// </summary>
        /// <param name="ids">List of unique attribute identifiers</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>List of attributes</returns>
        public List<T> Read(List<Guid> ids, TransactionOptions options = null)
        {
            return AttributeRepo.Read(ids, options);
        }
        /// <summary>
        /// Read single attribute
        /// </summary>
        /// <param name="id">Unique attribute identifier</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>Attribute</returns>
        public T Read(Guid id, TransactionOptions options = null)
        {
            return Read(new List<Guid>() { id }, options).FirstOrDefault();
        }
        /// <summary>
        /// Read all attribute records, or specify an offset and limit
        /// </summary>
        /// <param name="offset">Number of attribute records to skip</param>
        /// <param name="limit">Maximum number of attribute records to return</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>List of attributes</returns>
        public List<T> Read(int? offset = null, int? limit = null, TransactionOptions options = null)
        {
            return AttributeRepo.Read(offset, limit, options);
        }
        /// <summary>
        /// Read all items containing attribute record, or specify an offset and limit
        /// </summary>
        /// <param name="offset">Number of items to skip</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>List of items</returns>
        public List<Item> ReadItem(int? offset = null, int? limit = null, TransactionOptions options = null)
        {
            var itemIds = Read(offset, limit, options)
                .Select(x => x.ItemId)
                .ToList();
            return ItemRepo.Read(itemIds, options);
        }
        /// <summary>
        /// Read attribute associated with item
        /// </summary>
        /// <param name="item">Item to read attribute of</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>Attribute associated with item</returns>
        public T Read(Item item, TransactionOptions options = null)
        {
            return AttributeRepo.Read(item, options);
        }
        /// <summary>
        /// Update multiple attributes
        /// </summary>
        /// <remarks>
        /// All other forms of update should call this one.
        /// </remarks>
        /// <param name="attributes">List of attributes to update</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>List of updated attributes</returns>
        public List<T> Update(List<T> attributes, TransactionOptions options = null)
        {
            return AttributeRepo.Update(attributes, options);
        }
        /// <summary>
        /// Update single attribute
        /// </summary>
        /// <param name="model">Attributes to update</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>Updated attribute</returns>
        public T Update(T attribute, TransactionOptions options = null)
        {
            return Update(new List<T>() { attribute }, options).FirstOrDefault();
        }
        /// <summary>
        /// Delete multiple attributes
        /// </summary>
        /// <remarks>
        /// All other forms of delete should call this one
        /// </remarks>
        /// <param name="attributes">Attributes to delete</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void Delete(List<T> attributes, TransactionOptions options = null)
        {
            AttributeRepo.Delete(attributes, options);
        }
        /// <summary>
        /// Delete single attribute
        /// </summary>
        /// <param name="attribute">Attribute to delete</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void Delete(T model, TransactionOptions options = null)
        {
            Delete(new List<T>() { model }, options);
        }
        /// <summary>
        /// Delete all attributes
        /// </summary>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void Delete(TransactionOptions options = null)
        {
            AttributeRepo.Delete();
        }
        /// <summary>
        /// Determine if an attribute type should be managed by this manager
        /// </summary>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        public bool IsManaging(Type attributeType)
        {
            return attributeType == typeof(T);
        }
        /// <summary>
        /// Attach new attributes to an existing item
        /// </summary>
        /// <param name="attributes">Unsaved new attributes</param>
        /// <param name="item">Existing item that has already been saved</param>
        /// <returns></returns>
        public T Attach(T attributes, Item item)
        {
            attributes.Item = item;
            attributes = AttributeRepo.Create(attributes);
            return attributes;
        }
    }
}
