﻿using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Items.Attributes;
using BeforeOurTime.Models.Items.Attributes.Games;
using BeforeOurTime.Models.Items.Attributes.Locations;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Games
{
    public interface IGameAttributeManager : IAttributeManager<GameAttribute>, IAttributeManager
    {
        /// <summary>
        /// Get the default game
        /// </summary>
        /// <remarks>
        /// Will create the default game if one does not already exist
        /// </remarks>
        /// <returns></returns>
        Item GetDefaultGame();
        /// <summary>
        /// Get the default location of the default game
        /// </summary>
        /// <remarks>
        /// Default locations are used as the parent for item's whos parent is
        /// not specified
        /// </remarks>
        /// <returns></returns>
        LocationAttribute GetDefaultLocation();
        /// <summary>
        /// Update games's name
        /// </summary>
        /// <param name="id">Unique game attribute identifier</param>
        /// <param name="name">Game's new name</param>
        /// <returns></returns>
        GameAttribute UpdateName(Guid id, string name);
        /// <summary>
        /// Update games's default location
        /// </summary>
        /// <param name="id">Unique game attribute identifier</param>
        /// <param name="locationId">Game's new default location</param>
        /// <returns></returns>
        GameAttribute UpdateDefaultLocation(Guid id, Guid locationId);
    }
}