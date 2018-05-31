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
