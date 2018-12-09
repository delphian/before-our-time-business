using BeforeOurTime.Business.Dbs.EF;
using BeforeOurTime.Business.Modules.World.Dbs.EF;
using BeforeOurTime.Models.Modules.Core.Dbs;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.World.ItemProperties.Characters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Modules.World.ItemProperties.Characters
{
    /// <summary>
    /// Central data repository for all character items
    /// </summary>
    public class EFCharacterItemDataRepo : Repository<CharacterItemData>, ICharacterItemDataRepo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">Entity framework database context</param>
        public EFCharacterItemDataRepo(
            EFWorldModuleContext db,
            IItemRepo itemRepo) : base(db, db.GetDbSet<CharacterItemData>())
        {
        }
        /// <summary>
        /// Read associated character data of item id
        /// </summary>
        /// <param name="itemId">Item id that may have associated data</param>
        /// <returns></returns>
        public CharacterItemData ReadItemId(Guid itemId)
        {
            var data = Set.Where(x => x.DataItemId == itemId).Select(x => x.Id).FirstOrDefault();
            return Read(data);
        }
        /// <summary>
        /// Read associated Location attributes of item
        /// </summary>
        /// <param name="item">Item that may have associated attributes</param>
        /// <returns></returns>
        public CharacterItemData Read(Item item)
        {
            return ReadItemId(item.Id);
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
