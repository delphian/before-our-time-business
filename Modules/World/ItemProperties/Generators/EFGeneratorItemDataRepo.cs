﻿using BeforeOurTime.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System;
using BeforeOurTime.Models.Modules.Core.Dbs;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Business.Dbs.EF;
using BeforeOurTime.Business.Modules.World.Dbs.EF;
using BeforeOurTime.Models.Modules.World.ItemProperties.Generators;

namespace BeforeOurTime.Business.Modules.World.ItemProperties.Generators
{
    /// <summary>
    /// Access to generator item data in the data store
    /// </summary>
    public class EFGeneratorItemDataRepo : Repository<GeneratorItemData>, IGeneratorItemDataRepo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">Entity framework database context</param>
        public EFGeneratorItemDataRepo(
            EFWorldModuleContext db,
            IItemRepo itemRepo) : base(db, db.GetDbSet<GeneratorItemData>())
        {
        }
        /// <summary>
        /// Read associated generator item data of item
        /// </summary>
        /// <param name="item">Item that may have associated attributes</param>
        /// <returns></returns>
        public GeneratorItemData Read(Item item)
        {
            var dataId = Set.Where(x => x.DataItemId == item.Id).Select(x => x.Id).FirstOrDefault();
            return Read(dataId);
        }
        /// <summary>
        /// Read generator data for all generators ready to run
        /// </summary>
        /// <param name="futureTime">Optional future time. Defaults to now</param>
        /// <returns></returns>
        public List<GeneratorItemData> ReadReadyToRun(DateTime? futureTime = null)
        {
            var runBeforeTime = futureTime ?? DateTime.Now;
            var readyToRun = new List<GeneratorItemData>();
            var dataIds = Set.Where(x => 
                                    x.IntervalTime == null ||
                                    x.IntervalTime <= runBeforeTime)
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
