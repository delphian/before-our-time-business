using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Details;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Details
{
    public interface IDetailPhysicalManager : IDetailManager<DetailPhysical>, IDetailManager
    {
        /// <summary>
        /// Attach new physical attributes to an existing item
        /// </summary>
        /// <param name="item">Existing item that has already been saved</param>
        /// <param name="name">One, two, or three word short description of item</param>
        /// <param name="description">A long description of the item. Include many sensory experiences</param>
        /// <param name="volume">Volume</param>
        /// <param name="weight">Weight</param>
        DetailPhysical Attach(
            Item item,
            string name,
            string description,
            int volume,
            int weight);
        /// <summary>
        /// Create new item with new physical attributes
        /// </summary>
        /// <param name="parent">Parent item</param>
        /// <param name="name">One, two, or three word short description of item</param>
        /// <param name="description">A long description of the item. Include many sensory experiences</param>
        /// <param name="volume">Volume</param>
        /// <param name="weight">Weight</param>
        DetailPhysical Create(
            Item parent,
            string name,
            string description,
            int volume,
            int weight);
    }
}
