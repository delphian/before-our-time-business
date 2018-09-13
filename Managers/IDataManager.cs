using BeforeOurTime.Business.Dbs;
using BeforeOurTime.Business.Models;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Managers
{
    public interface IDataManager
    {
        /// <summary>
        /// Read all items containing attribute record, or specify an offset and limit
        /// </summary>
        /// <param name="offset">Number of items to skip</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>List of items</returns>
        List<Item> ReadItem(int? offset = null, int? limit = null, TransactionOptions options = null);
        /// <summary>
        /// Determine if an item has attributes that may be managed
        /// </summary>
        /// <param name="item">Item that may posses attributes</param>
        bool IsManaging(Item item);
        /// <summary>
        /// Determine if an attribute type should be managed by this manager
        /// </summary>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        bool IsManaging(Type attributeType);
    }
    /// <summary>
    /// Manage details of an item's extended data
    /// </summary>
    public interface IDataManager<T> : ICrudDataRepository<T> where T : Data
    {
        /// <summary>
        /// Create many models with base item
        /// </summary>
        /// <param name="models"></param>
        /// <param name="parentId">Create the base items as children of this parent item</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns></returns>
        List<Item> Create(List<T> models, Guid parentId, TransactionOptions options = null);
        /// <summary>
        /// Create model with base item
        /// </summary>
        /// <param name="model"></param>
        /// <param name="parentId">Create the base items as children of this parent item</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns></returns>
        Item Create(T model, Guid parentId, TransactionOptions options = null);
        /// <summary>
        /// Attach new attributes to an existing item
        /// </summary>
        /// <param name="attributes">Unsaved new attributes</param>
        /// <param name="item">Existing item that has already been saved</param>
        /// <returns></returns>
        T Attach(T attributes, Item item);
    }
}
