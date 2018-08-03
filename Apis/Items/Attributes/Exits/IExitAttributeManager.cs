using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Items.Attributes;
using BeforeOurTime.Models.Items.Attributes.Exits;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Exits
{
    public interface IExitAttributeManager : IAttributeManager<ExitAttribute>, IAttributeManager
    {
        /// <summary>
        /// Update the attribute name
        /// </summary>
        /// <param name="id">Unique attribute identifier</param>
        /// <param name="name">New name of the attribute</param>
        /// <returns></returns>
        ExitAttribute UpdateName(Guid id, string name);
        /// <summary>
        /// Update the attribute description
        /// </summary>
        /// <param name="id">Unique attribute identifier</param>
        /// <param name="description">New description of the attribute</param>
        /// <returns></returns>
        ExitAttribute UpdateDescription(Guid id, string description);
        /// <summary>
        /// Update the destination location
        /// </summary>
        /// <param name="id">Unique exit attribute identifier</param>
        /// <param name="destinationLocationId">New location id of the exit destination</param>
        /// <returns></returns>
        ExitAttribute UpdateDestination(
            Guid id, 
            Guid destinationLocationId);
    }
}
