using BeforeOurTime.Models;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.ItemAttributes;
using BeforeOurTime.Models.ItemAttributes.Locations;
using BeforeOutTime.Repository.Dbs.EF;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Repository.Dbs.EF.Items.Attributes
{
    /// <summary>
    /// Central data repository for all location items
    /// </summary>
    public class LocationAttributeRepo : Repository<LocationAttribute>, ILocationAttributeRepo
    {
        /// <summary>
        /// Table that repository will operate with
        /// </summary>
        protected DbSet<LocationAttribute> Set { set; get; }
        /// <summary>
        /// Item repository
        /// </summary>
        protected IItemRepo ItemRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">Entity framework database context</param>
        public LocationAttributeRepo(
            BaseContext db,
            IItemRepo itemRepo) : base(db)
        {
            Set = db.GetDbSet<LocationAttribute>();
            ItemRepo = itemRepo;
            ItemRepo.OnItemCreate += OnItemCreate;
            ItemRepo.OnItemRead += OnItemRead;
            ItemRepo.OnItemUpdate += OnItemUpdate;
            ItemRepo.OnItemDelete += OnItemDelete;
        }
        /// <summary>
        /// Read associated location attributes of item
        /// </summary>
        /// <remmarks>
        /// Location will return associated item and all that item's children
        /// </remmarks>
        /// <param name="item">Item that may have associated attributes</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns></returns>
        public LocationAttribute Read(Item item, TransactionOptions options = null)
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
            if (item.HasAttribute<LocationAttribute>())
            {
                var attribute = item.GetAttribute<LocationAttribute>();
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
            if (item.HasAttribute<LocationAttribute>())
            {
                var attribute = item.GetAttribute<LocationAttribute>();
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
            if (item.HasAttribute<LocationAttribute>())
            {
                var attribute = item.GetAttribute<LocationAttribute>();
                base.Delete(attribute, options);
            }
        }
    }
}
