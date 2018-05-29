using BeforeOurTime.Business.Apis.Scripts.Libraries;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Details;
using BeforeOurTime.Repository.Models.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Details
{
    public interface IDetailManager
    {
        /// <summary>
        /// Get the item type that the manager is responsible for providing detail management for
        /// </summary>
        /// <returns></returns>
        ItemType GetItemType();
        /// <summary>
        /// Deliver a message to an item
        /// </summary>
        /// <remarks>
        /// Often results in the item's script executing and parsing the message package
        /// </remarks>
        /// <param name="item"></param>
        void DeliverMessage(Message message, Item item, JsFunctionManager jsFunctionManager);
        /// <summary>
        /// Determine if an item has attributes that may be managed
        /// </summary>
        /// <param name="item">Item that may posses attributes</param>
        bool IsManaging(Item item);
    }
    /// <summary>
    /// Manage details of an item's extended data
    /// </summary>
    public interface IDetailManager<T> : IDetailManager where T: IDetail
    {
        /// <summary>
        /// Attach new attributes to an existing item
        /// </summary>
        /// <param name="attributes">Unsaved new attributes</param>
        /// <param name="item">Existing item that has already been saved</param>
        /// <returns></returns>
        T Attach(T attributes, Item item);
        /// <summary>
        /// Read a single item with game attributes
        /// </summary>
        /// <param name="id">Unique model identifier</param>
        /// <returns></returns>
        T Read(Guid id);
        /// <summary>
        /// Read all items with game attributes, or specify an offset and limit
        /// </summary>
        /// <param name="offset">Number of records to skip</param>
        /// <param name="limit">Maximum number of records to return</param>
        /// <returns></returns>
        List<T> Read(int? offset = null, int? limit = null);
    }
}
