using BeforeOurTime.Repository.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items
{
    public interface IItemManager
    {
        /// <summary>
        /// Create a new item
        /// </summary>
        /// <remarks>
        /// Any item, or model derived from an item, may be created
        /// </remarks>
        /// <param name="source">Item responsible for doing the creating</param>
        /// <param name="item">Item which is new and being created</param>
        Item Create(Item source, Item item);
        /// <summary>
        /// Read single model of a type derived from Item
        /// </summary>
        /// <param name="itemIds">Unique item identifier</param>
        /// <returns></returns>
        Item Read(Guid itemId);
        /// <summary>
        /// Read multiple models of a type derived from Item
        /// </summary>
        /// <param name="itemIds">List of unique item identifiers</param>
        /// <returns></returns>
        List<Item> Read(List<Guid> itemIds);
        /// <summary>
        /// Read all models of a type derived from Item, or specify an offset and limit
        /// </summary>
        /// <remarks>
        /// If repository was instanted with type Item (T) then this method may be used to 
        /// read any model (TDerived) that is derived from type T (Item)
        /// </remarks>
        /// <param name="offset">Number of model records to skip</param>
        /// <param name="limit">Maximum number of model records to return</param>
        /// <returns></returns>
        List<Item> Read(int? offset = null, int? limit = null);
        /// <summary>
        /// Update any model that is derived from type Item
        /// </summary>
        /// <param name="item">Item to be updated</param>
        /// <returns></returns>
        Item Update(Item item);
        /// <summary>
        /// Permenantly delete an item and remove from data store
        /// </summary>
        /// <remarks>
        /// All children will be re-homed to the item parent unless otherwise specified
        /// </remarks>
        /// <param name="item">Item to delete</param>
        /// <param name="deleteChildren">Also delete all children</param>
        void Delete(Item item, bool? deleteChildren = false);
        /// <summary>
        /// Relocate an item
        /// </summary>
        /// <param name="item">Item to move</param>
        /// <param name="newParent">Item which will become the parent</param>
        /// <param name="source">Item responsible for doing the moving</param>
        Item Move(Item item, Item newParent, Item source = null);
    }
}
