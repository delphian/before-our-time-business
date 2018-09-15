using BeforeOurTime.Business.Dbs;
using BeforeOurTime.Business.Modules.Core.Models.Data;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Modules.Core.Dbs
{
    /// <summary>
    /// Central data repository for all game items
    /// </summary>
    public interface IGameDataRepo : ICrudDataRepository, ICrudDataRepository<GameData>
    {
        /// <summary>
        /// Read associated game attributes of item
        /// </summary>
        /// <param name="item">Item that may have associated attributes</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns></returns>
        GameData Read(Item item, TransactionOptions options = null);
    }
}
