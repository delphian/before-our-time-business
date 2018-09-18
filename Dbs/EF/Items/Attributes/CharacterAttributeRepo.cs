using BeforeOurTime.Models;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.ItemAttributes.Characters;
using BeforeOutTime.Repository.Dbs.EF;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace BeforeOurTime.Repository.Dbs.EF.Items.Attributes
{
    /// <summary>
    /// Central data repository for all exit items
    /// </summary>
    public class CharacterAttributeRepo : Repository<CharacterAttribute>, ICharacterAttributeRepo
    {
        /// <summary>
        /// Table that repository will operate with
        /// </summary>
        protected DbSet<CharacterAttribute> Set { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">Entity framework database context</param>
        public CharacterAttributeRepo(
            BaseContext db,
            IItemRepo itemRepo) : base(db)
        {
            Set = db.GetDbSet<CharacterAttribute>();
            itemRepo.OnItemCreate += OnItemCreate;
            itemRepo.OnItemRead += OnItemRead;
            itemRepo.OnItemUpdate += OnItemUpdate;
            itemRepo.OnItemDelete += OnItemDelete;
        }
        /// <summary>
        /// Read associated exit attributes of item
        /// </summary>
        /// <param name="item">Item that may have associated attributes</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns></returns>
        public CharacterAttribute Read(Item item, TransactionOptions options = null)
        {
            var attributeId = Set.Where(x => x.ItemId == item.Id).Select(x => x.Id).FirstOrDefault();
            return base.Read(attributeId, options);
        }
        /// <summary>
        /// Create attribute, if present, after item is created
        /// </summary>
        /// <param name="item">Base item just created from datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemCreate(Item item, TransactionOptions options = null)
        {
            if (item.HasAttribute<CharacterAttribute>())
            {
                var attribute = item.GetAttribute<CharacterAttribute>();
                attribute.ItemId = item.Id;
                base.Create(attribute, options);
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item just read from datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemRead(Item item, TransactionOptions options = null)
        {
            var attributes = Read(item, options);
            if (attributes != null)
            {
                item.Attributes.Add(attributes);
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item about to be persisted to datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemUpdate(Item item, TransactionOptions options = null)
        {
            if (item.HasAttribute<CharacterAttribute>())
            {
                var attribute = item.GetAttribute<CharacterAttribute>();
                base.Update(attribute, options);
            }
        }
        /// <summary>
        /// Delete attribute of base item before base item is deleted
        /// </summary>
        /// <param name="item">Base item about to be deleted</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemDelete(Item item, TransactionOptions options = null)
        {
            if (item.HasAttribute<CharacterAttribute>())
            {
                var attribute = item.GetAttribute<CharacterAttribute>();
                base.Delete(attribute, options);
            }
        }
    }
}
