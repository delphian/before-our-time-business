using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Details;
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
    }
    /// <summary>
    /// Manage details of an item's extended data
    /// </summary>
    public interface IDetailManager<T> : IDetailManager where T: IDetail
    {
    }
}
