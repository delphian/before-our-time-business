using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.ItemAttributes;
using BeforeOurTime.Models.ItemAttributes.Characters;
using BeforeOurTime.Models.ItemAttributes.Locations;
using BeforeOurTime.Models.ItemAttributes.Physicals;
using BeforeOurTime.Models.ItemAttributes.Players;
using BeforeOurTime.Models.Items.Players;
using System;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Models.ItemAttributes.Visibles;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Players
{
    public interface IPlayerAttributeManager : IAttributeManager<PlayerAttribute>, IAttributeManager
    {
        /// <summary>
        /// Create a new player
        /// </summary>
        /// <param name="character">Attributes describing how an item looks or is percieved</param>
        /// <param name="characer">Properties generally understood to denote a state of being alive</param>
        /// <param name="physical">Physical attributes</param>
        /// <param name="player">Player attributes</param>
        /// <param name="initialLocation">Location of new player</param>
        PlayerItem Create(
            VisibleAttribute visible,
            CharacterAttribute character,
            PhysicalAttribute physical,
            PlayerAttribute player,
            Guid defaultLocationId);
    }
}
