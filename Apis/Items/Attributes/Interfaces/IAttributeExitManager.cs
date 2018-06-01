using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Attributes;
using BeforeOurTime.Repository.Models.Items.Attributes.Exits;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Interfaces
{
    public interface IAttributeExitManager : IAttributeManager<AttributeExit>, IAttributeManager
    {
        /// <summary>
        /// Update the attribute name
        /// </summary>
        /// <param name="id">Unique attribute identifier</param>
        /// <param name="name">New name of the attribute</param>
        /// <returns></returns>
        AttributeExit UpdateName(Guid id, string name);
        /// <summary>
        /// Update the attribute description
        /// </summary>
        /// <param name="id">Unique attribute identifier</param>
        /// <param name="description">New description of the attribute</param>
        /// <returns></returns>
        AttributeExit UpdateDescription(Guid id, string description);
        /// <summary>
        /// Update the destination location
        /// </summary>
        /// <param name="id">Unique exit attribute identifier</param>
        /// <param name="destinationLocationId">New location id of the exit destination</param>
        /// <returns></returns>
        AttributeExit UpdateDestination(
            Guid id, 
            Guid destinationLocationId);
    }
}
