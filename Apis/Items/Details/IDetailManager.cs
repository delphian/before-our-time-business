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
    }
    /// <summary>
    /// Manage details of an item's extended data
    /// </summary>
    public interface IDetailManager<T> : IDetailManager where T: IDetail
    {
    }
}
