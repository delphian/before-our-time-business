using BeforeOurTime.Business.Apis.Items.Attributes.Interfaces;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Items.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Locations
{
    public interface ILocationAttributeManager : IAttributeManager<AttributeLocation>, IAttributeManager
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
        new AttributeLocation Read(Item item);
        /// <summary>
        /// Create an empty new location and connecting exits from a provided location
        /// </summary>
        /// <param name="currentLocationItemId">Existing location item to link to new location with exits</param>
        /// <returns></returns>
        Item CreateFromHere(Guid currentLocationItemId);
        /// <summary>
        /// Update attribute's name
        /// </summary>
        /// <param name="id">Unique attribute identifier</param>
        /// <param name="name">Attribute's new name</param>
        /// <returns></returns>
        AttributeLocation UpdateName(Guid id, string name);
        /// <summary>
        /// Update attribute's description
        /// </summary>
        /// <param name="id">Unique attribute identifier</param>
        /// <param name="description">Attribute's new description</param>
        /// <returns></returns>
        AttributeLocation UpdateDescription(Guid id, string description);
    }
}
