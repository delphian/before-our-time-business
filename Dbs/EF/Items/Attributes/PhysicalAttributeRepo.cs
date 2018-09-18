using BeforeOurTime.Models;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.ItemAttributes;
using BeforeOurTime.Models.ItemAttributes.Physicals;
using BeforeOutTime.Repository.Dbs.EF;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeforeOurTime.Repository.Dbs.EF.Items.Attributes
{
    /// <summary>
    /// Central data repository for all game items
    /// </summary>
    public class PhysicalAttributeRepo : Repository<PhysicalAttribute>, IPhysicalAttributeRepo
    {
        /// <summary>
        /// Table that repository will operate with
        /// </summary>
        protected DbSet<PhysicalAttribute> Set { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">Entity framework database context</param>
        public PhysicalAttributeRepo(
            BaseContext db, 
            IItemRepo itemRepo) : base(db)
        {
            Set = db.GetDbSet<PhysicalAttribute>();
            itemRepo.OnItemCreate += OnItemCreate;
            itemRepo.OnItemRead += OnItemRead;
            itemRepo.OnItemUpdate += OnItemUpdate;
            itemRepo.OnItemDelete += OnItemDelete;
        }
        /// <summary>
        /// Read a list of physical attributes
        /// </summary>
        /// <remarks>
        /// All other forms of read should call this one.
        /// Override to force inclusion of child properties
        /// </remarks>
        /// <param name="ids">List of unique physical attribute identifiers</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public override List<PhysicalAttribute> Read(List<Guid> ids, TransactionOptions options = null)
        {
            options = options ?? new TransactionOptions()
            {
                NoTracking = true
            };
            var resultSet = Set
                .Where(x => ids.Contains(x.Id))
                .Include(x => x.ImageIcon)
                .AsQueryable();
            resultSet = (options?.NoTracking == true) ? resultSet.AsNoTracking() : resultSet;
            return resultSet.ToList();
        }
        /// <summary>
        /// Read associated physical attributes of item
        /// </summary>
        /// <param name="item">Item that may have associated attributes</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns></returns>
        public PhysicalAttribute Read(Item item, TransactionOptions options = null)
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
            if (item.HasAttribute<PhysicalAttribute>())
            {
                var attribute = item.GetAttribute<PhysicalAttribute>();
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
            if (item.HasAttribute<PhysicalAttribute>())
            {
                var attribute = item.GetAttribute<PhysicalAttribute>();
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
            if (item.HasAttribute<PhysicalAttribute>())
            {
                var attribute = item.GetAttribute<PhysicalAttribute>();
                base.Delete(attribute, options);
            }
        }
    }
}
