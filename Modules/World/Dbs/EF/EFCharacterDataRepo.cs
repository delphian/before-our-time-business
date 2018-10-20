using BeforeOurTime.Business.Dbs.EF;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Modules.Core.Dbs;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.World.Dbs;
using BeforeOurTime.Models.Modules.World.Models.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Modules.World.Dbs.EF
{
    /// <summary>
    /// Central data repository for all character items
    /// </summary>
    public class EFCharacterDataRepo : Repository<CharacterData>, ICharacterDataRepo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">Entity framework database context</param>
        public EFCharacterDataRepo(
            EFWorldModuleContext db,
            IItemRepo itemRepo) : base(db, db.GetDbSet<CharacterData>())
        {
        }
        /// <summary>
        /// Read associated Location attributes of item
        /// </summary>
        /// <param name="item">Item that may have associated attributes</param>
        /// <returns></returns>
        public CharacterData Read(Item item)
        {
            var dataId = Set.Where(x => x.DataItemId == item.Id).Select(x => x.Id).FirstOrDefault();
            return Read(dataId);
        }
        /// <summary>
        /// Get all unique item identifiers of managed items
        /// </summary>
        /// <returns></returns>
        public List<Guid> GetItemIds()
        {
            return Set.Select(x => x.DataItemId).ToList();
        }
    }
}
