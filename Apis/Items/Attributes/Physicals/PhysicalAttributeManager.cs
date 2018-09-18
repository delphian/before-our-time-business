using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.ItemAttributes;
using BeforeOurTime.Models.ItemAttributes.Physicals;
using BeforeOurTime.Models.Primitives.Images;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Physicals
{
    public class PhysicalAttributeManager : AttributeManager<PhysicalAttribute>, IPhysicalAttributeManager
    {
        private IPhysicalAttributeRepo DetailPhysicalRepo { set; get; }
        private IItemManager ItemManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public PhysicalAttributeManager(
            IItemRepo itemRepo,
            IPhysicalAttributeRepo detailPhysicalRepo,
            IItemManager itemManager) : base(itemRepo, detailPhysicalRepo)
        {
            DetailPhysicalRepo = detailPhysicalRepo;
            ItemManager = itemManager;
        }
        /// <summary>
        /// Attach new physical attributes to an existing item
        /// </summary>
        /// <param name="item">Existing item that has already been saved</param>
        /// <param name="name">One, two, or three word short description of item</param>
        /// <param name="description">A long description of the item. Include many sensory experiences</param>
        /// <param name="height">Height</param>
        /// <param name="weight">Weight</param>
        public PhysicalAttribute Attach(
            Item item,
            string name,
            string description,
            int height,
            int weight)
        {
            var physicalAttributes = new PhysicalAttribute()
            {
                Name = name,
                Description = description,
                Height = height,
                Weight = weight,
                Item = item
            };
            var physical = Attach(physicalAttributes, item);
            return physical;
        }
        /// <summary>
        /// Create new item with new physical attributes
        /// </summary>
        /// <param name="parent">Parent item</param>
        /// <param name="name">One, two, or three word short description of item</param>
        /// <param name="description">A long description of the item. Include many sensory experiences</param>
        /// <param name="volume">Volume</param>
        /// <param name="weight">Weight</param>
        public PhysicalAttribute Create(
            Item parent,
            string name,
            string description,
            int volume,
            int weight)
        {
            var item = ItemManager.Create(new Item()
            {
                ParentId = parent.Id,
            });
            var physical = Attach(item, name, description, volume, weight);
            return physical;
        }
        /// <summary>
        /// Determine if an item has attributes that may be managed
        /// </summary>
        /// <param name="item">Item that may posses attributes</param>
        public bool IsManaging(Item item)
        {
            var managed = false;
            if (DetailPhysicalRepo.Read(item) != null) {
                managed = true;
            }
            return managed;
        }
        /// <summary>
        /// Update the physical name
        /// </summary>
        /// <param name="id">Unique phsyical attribute identifier</param>
        /// <param name="description">New name of the physical</param>
        /// <returns></returns>
        public PhysicalAttribute UpdateName(Guid id, string name)
        {
            var physicalAttribute = Read(id);
            physicalAttribute.Name = name;
            return Update(physicalAttribute);
        }
        /// <summary>
        /// Update the physical description
        /// </summary>
        /// <param name="id">Unique phsyical attribute identifier</param>
        /// <param name="description">New description of the physical</param>
        /// <returns></returns>
        public PhysicalAttribute UpdateDescription(Guid id, string description)
        {
            var physicalAttribute = Read(id);
            physicalAttribute.Description = description;
            return Update(physicalAttribute);
        }
        /// <summary>
        /// Update the physical attribute's image icon
        /// </summary>
        /// <param name="id">Unique phsyical attribute identifier</param>
        /// <param name="image">Wrapper for image that allows meta data</param>
        /// <returns></returns>
        public PhysicalAttribute UpdateImageIcon(Guid id, Image image)
        {
            var physicalAttribute = Read(id);
            physicalAttribute.ImageIcon = image;
            return Update(physicalAttribute);
        }
        /// <summary>
        /// Update the physical volume
        /// </summary>
        /// <param name="id">Unique phsyical attribute identifier</param>
        /// <param name="volume">New height of the physical</param>
        /// <returns></returns>
        public PhysicalAttribute UpdateHeight(Guid id, int height)
        {
            var physicalAttribute = Read(id);
            physicalAttribute.Height = height;
            return Update(physicalAttribute);
        }
        /// <summary>
        /// Update the physical weight
        /// </summary>
        /// <param name="id">Unique phsyical attribute identifier</param>
        /// <param name="weight">New weight of the physical</param>
        /// <returns></returns>
        public PhysicalAttribute UpdateWeight(Guid id, int weight)
        {
            var physicalAttribute = Read(id);
            physicalAttribute.Weight = weight;
            return Update(physicalAttribute);
        }
    }
}
