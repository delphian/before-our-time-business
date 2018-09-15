using BeforeOurTime.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Modules.Core
{
    public interface ICoreModule : IModule
    {
        /// <summary>
        /// Get the default game
        /// </summary>
        /// <remarks>
        /// Will create the default game and a location if one does not already exist
        /// </remarks>
        /// <returns></returns>
        Item GetDefaultGame();
        /// <summary>
        /// Get default game location
        /// </summary>
        /// <returns></returns>
        Item GetDefaultLocation();
    }
}
