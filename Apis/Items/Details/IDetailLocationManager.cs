using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Details;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Details
{
    public interface IDetailLocationManager : IDetailManager<DetailLocation>, IDetailManager
    {
        /// <summary>
        /// Read item's detailed location
        /// </summary>
        /// <remarks>
        /// If item itself has no location detail then item's parents will be 
        /// traversed until one is found
        /// </remarks>
        /// <param name="item">Item that has attached detail location data</param>
        /// <returns>The Item's detailed location data. Null if none found</returns>
        new DetailLocation Read(Item item);
        /// <summary>
        /// Update location's name
        /// </summary>
        /// <param name="id">Unique location attribute identifier</param>
        /// <param name="name">Location's new name</param>
        /// <returns></returns>
        DetailLocation UpdateName(Guid id, string name);
    }
}
