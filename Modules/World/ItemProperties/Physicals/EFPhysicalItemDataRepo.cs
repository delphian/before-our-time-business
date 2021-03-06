﻿using BeforeOurTime.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System;
using BeforeOurTime.Models.Modules.Core.Dbs;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Business.Dbs.EF;
using BeforeOurTime.Models.Modules.World.ItemProperties.Physicals;
using BeforeOurTime.Business.Modules.World.Dbs.EF;

namespace BeforeOurTime.Business.Modules.World.ItemProperties.Physicals
{
    /// <summary>
    /// Access to Game Data in the data store
    /// </summary>
    public class EFPhysicalItemDataRepo : Repository<PhysicalItemData>, IPhysicalItemDataRepo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">Entity framework database context</param>
        public EFPhysicalItemDataRepo(
            EFWorldModuleContext db,
            IItemRepo itemRepo) : base(db, db.GetDbSet<PhysicalItemData>())
        {
        }
        /// <summary>
        /// Read associated game attributes of item
        /// </summary>
        /// <param name="item">Item that may have associated attributes</param>
        /// <returns></returns>
        public PhysicalItemData Read(Item item)
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
