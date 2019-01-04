using BeforeOurTime.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System;
using BeforeOurTime.Models.Modules.Core.Dbs;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Business.Dbs.EF;
using BeforeOurTime.Models.Modules.Script.ItemProperties.Javascripts;
using BeforeOurTime.Business.Modules.Script.Dbs.EF;

namespace BeforeOurTime.Business.Modules.Script.ItemProperties.Javascripts
{
    /// <summary>
    /// Access to javascript item data in the data store
    /// </summary>
    public class EFJavascriptItemDataRepo : Repository<JavascriptItemData>, IJavascriptItemDataRepo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">Entity framework database context</param>
        public EFJavascriptItemDataRepo(
            EFScriptModuleContext db,
            IItemRepo itemRepo) : base(db, db.GetDbSet<JavascriptItemData>())
        {
        }
        /// <summary>
        /// Read associated javascript item data of item
        /// </summary>
        /// <param name="item">Item that may have associated attributes</param>
        /// <returns></returns>
        public JavascriptItemData Read(Item item)
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
