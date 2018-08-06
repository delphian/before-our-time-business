using BeforeOurTime.Business.Apis.Scripts.Libraries;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Items.Attributes;
using BeforeOurTime.Repository.Models;
using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Messages.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes
{
    public interface IAttributeManager
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
        /// Deliver a message to an item
        /// </summary>
        /// <remarks>
        /// Often results in the item's script executing and parsing the message package
        /// </remarks>
        /// <param name="item"></param>
        void DeliverMessage(SavedMessage message, Item item, JsFunctionManager jsFunctionManager);
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
    public interface IAttributeManager<T> : IAttributeRepo<T> where T: ItemAttribute
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
