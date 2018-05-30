using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Details;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Interfaces
{
    public interface IAttributeGameManager : IAttributeManager<DetailGame>, IAttributeManager
    {
        /// <summary>
        /// Get the default game
        /// </summary>
        /// <remarks>
        /// Will create the default game if one does not already exist
        /// </remarks>
        /// <returns></returns>
        DetailGame GetDefaultGame();
        /// <summary>
        /// Get the default location of the default game
        /// </summary>
        /// <remarks>
        /// Default locations are used as the parent for item's whos parent is
        /// not specified
        /// </remarks>
        /// <returns></returns>
        DetailLocation GetDefaultLocation();
        /// <summary>
        /// Update games's name
        /// </summary>
        /// <param name="id">Unique game attribute identifier</param>
        /// <param name="name">Game's new name</param>
        /// <returns></returns>
        DetailGame UpdateName(Guid id, string name);
    }
}
