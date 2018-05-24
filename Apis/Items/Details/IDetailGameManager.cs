using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Details;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Details
{
    public interface IDetailGameManager : IDetailManager<DetailGame>
    {
        /// <summary>
        /// Create a game item
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        DetailGame Create(DetailGame game);
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
    }
}
