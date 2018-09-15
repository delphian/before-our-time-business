using System;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Business.Managers;
using BeforeOurTime.Business.Modules.Core.Models.Data;

namespace BeforeOurTime.Business.Modules.Core.Managers
{
    public interface IGameDataManager : IDataManager, IDataManager<GameData>
    {
        /// <summary>
        /// Update games's default location
        /// </summary>
        /// <param name="id">Unique game attribute identifier</param>
        /// <param name="locationId">Game's new default location</param>
        /// <returns></returns>
        GameData UpdateDefaultLocation(Guid id, Guid locationId);
    }
}
