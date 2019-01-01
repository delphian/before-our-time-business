using BeforeOurTime.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System;
using BeforeOurTime.Models.Modules.Core.Dbs;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Business.Dbs.EF;
using BeforeOurTime.Business.Modules.World.Dbs.EF;
using BeforeOurTime.Models.Modules.World.ItemProperties.Generators;
using BeforeOurTime.Models.Modules.World.ItemProperties.Garbages;

namespace BeforeOurTime.Business.Modules.World.ItemProperties.Generators
{
    /// <summary>
    /// Access to generator item data in the data store
    /// </summary>
    public class EFGarbageItemDataRepo : Repository<GarbageItemData>, IGarbageItemDataRepo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">Entity framework database context</param>
        public EFGarbageItemDataRepo(
            EFWorldModuleContext db,
            IItemRepo itemRepo) : base(db, db.GetDbSet<GarbageItemData>())
        {
        }
        /// <summary>
        /// Read associated generator item data of item
        /// </summary>
        /// <param name="item">Item that may have associated attributes</param>
        /// <returns></returns>
        public GarbageItemData Read(Item item)
        {
            var dataId = Set.Where(x => x.DataItemId == item.Id).Select(x => x.Id).FirstOrDefault();
            return Read(dataId);
        }
        /// <summary>
        /// Read garbage data for all items ready to be collected
        /// </summary>
        /// <returns></returns>
        public List<GarbageItemData> ReadExpired()
        {
            var dataIds = Set.Where(x => x.IntervalTime >= DateTime.Now)
                .Select(x => x.Id).ToList();
            return Read(dataIds);
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
