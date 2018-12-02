using BeforeOurTime.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System;
using BeforeOurTime.Models.Modules.Core.Dbs;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Business.Dbs.EF;
using BeforeOurTime.Models.Modules.World.ItemProperties.Exits;
using BeforeOurTime.Business.Modules.World.Dbs.EF;

namespace BeforeOurTime.Business.Modules.World.ItemProperties.Exits
{
    /// <summary>
    /// Access to Game Data in the data store
    /// </summary>
    public class EFExitItemDataRepo : Repository<ExitItemData>, IExitItemDataRepo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">Entity framework database context</param>
        public EFExitItemDataRepo(
            EFWorldModuleContext db,
            IItemRepo itemRepo) : base(db, db.GetDbSet<ExitItemData>())
        {
        }
        /// <summary>
        /// Read associated game attributes of item
        /// </summary>
        /// <param name="item">Item that may have associated attributes</param>
        /// <returns></returns>
        public ExitItemData Read(Item item)
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
        /// <summary>
        /// Read all exit data that target the same destination
        /// </summary>
        /// <param name="desitnationId">Location that is the destination</param>
        /// <returns></returns>
        public List<ExitItemData> ReadDestinationId(Guid destinationId)
        {
            var attributeIds = Set
                .Where(x => x.DestinationLocationId == destinationId)
                .Select(x => x.Id)
                .ToList();
            var attributes = Read(attributeIds);
            return attributes;
        }
    }
}
