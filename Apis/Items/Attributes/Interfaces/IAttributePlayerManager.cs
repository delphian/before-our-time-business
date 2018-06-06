using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Interfaces
{
    public interface IAttributePlayerManager : IAttributeManager<AttributePlayer>, IAttributeManager
    {
        /// <summary>
        /// Create a new player
        /// </summary>
        /// <remarks>
        /// Creates new item and attaches all attributes
        /// </remarks>
        /// <param name="name">Public name of the player</param>
        /// <param name="accountId">Account to which this player belongs</param>
        /// <param name="physical">Physical attributes</param>
        /// <param name="initialLocation">Location of new player</param>
        AttributePlayer Create(
            string name, 
            Guid accountId, 
            AttributePhysical physical,
            AttributeLocation initialLocation);
    }
}
