using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.ItemAttributes;
using BeforeOurTime.Models.ItemAttributes.Locations;
using BeforeOurTime.Models.Items.Locations;
using BeforeOurTime.Repository.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Locations
{
    public interface ILocationAttributeManager : IAttributeManager<LocationAttribute>, IAttributeManager
    {
        /// <summary>
        /// Read item's detailed location
        /// </summary>
        /// <remarks>
        /// If item itself has no location detail then item's parents will be 
        /// traversed until one is found
        /// </remarks>
        /// <param name="item">Item that has attached detail location data</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>The Item's detailed location data. Null if none found</returns>
        new LocationAttribute Read(Item item, TransactionOptions options = null);
        /// <summary>
        /// Create an empty new location and connecting exits from a provided location
        /// </summary>
        /// <param name="currentLocationItemId">Existing location item to link to new location with exits</param>
        /// <returns></returns>
        LocationItem CreateFromHere(Guid currentLocationItemId);
        /// <summary>
        /// Update attribute's name
        /// </summary>
        /// <param name="id">Unique attribute identifier</param>
        /// <param name="name">Attribute's new name</param>
        /// <returns></returns>
        LocationAttribute UpdateName(Guid id, string name);
        /// <summary>
        /// Update attribute's description
        /// </summary>
        /// <param name="id">Unique attribute identifier</param>
        /// <param name="description">Attribute's new description</param>
        /// <returns></returns>
        LocationAttribute UpdateDescription(Guid id, string description);
    }
}
