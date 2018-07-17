using BeforeOurTime.Models.Items.Attributes;
using BeforeOurTime.Models.Items.Attributes.Players;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Interfaces
{
    public interface IPlayerAttributeManager : IAttributeManager<PlayerAttribute>, IAttributeManager
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
        PlayerAttribute Create(
            string name, 
            Guid accountId, 
            AttributePhysical physical,
            AttributeLocation initialLocation);
    }
}
