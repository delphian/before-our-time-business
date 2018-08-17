using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Items.Attributes;
using BeforeOurTime.Models.Items.Attributes.Characters;
using BeforeOurTime.Models.Items.Attributes.Locations;
using BeforeOurTime.Models.Items.Attributes.Physicals;
using BeforeOurTime.Models.Items.Attributes.Players;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Players
{
    public interface IPlayerAttributeManager : IAttributeManager<PlayerAttribute>, IAttributeManager
    {
        /// <summary>
        /// Create a new player
        /// </summary>
        /// <param name="characer">Properties generally understood to denote a state of being alive</param>
        /// <param name="physical">Physical attributes</param>
        /// <param name="player">Player attributes</param>
        /// <param name="initialLocation">Location of new player</param>
        Item Create(
            CharacterAttribute character,
            PhysicalAttribute physical,
            PlayerAttribute player,
            LocationAttribute initialLocation);
    }
}
