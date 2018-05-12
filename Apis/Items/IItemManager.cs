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
        bool Create<T>(Item source, T item) where T : Item;
    }
}
