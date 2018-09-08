using BeforeOurTime.Business.Apis.Items.Attributes.Exits;
using BeforeOurTime.Business.Apis.Items.Attributes.Games;
using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Business.Apis.Items.Attributes.Locations;
using BeforeOurTime.Business.Apis.Messages;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.ItemAttributes;
using BeforeOurTime.Repository.Models;
using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Messages.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Models.ItemAttributes.Exits;
using BeforeOurTime.Models.ItemAttributes.Locations;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Items.Locations;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Locations
{
    public class LocationAttributeManager : AttributeManager<LocationAttribute>, ILocationAttributeManager
    {
        private ILocationAttributeRepo DetailLocationRepo { set; get; }
        private IItemManager ItemManager { set; get; }
        private IGameAttributeManager GameAttributeManager { set; get; }
        private IExitAttributeManager ExitAttributeManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public LocationAttributeManager(
            IItemRepo itemRepo,
            ILocationAttributeRepo detailLocationRepo,
            IItemManager itemManager,
            IGameAttributeManager gameAttributeMangaer,
            IExitAttributeManager exitAttributeManager) : base(itemRepo, detailLocationRepo)
        {
            DetailLocationRepo = detailLocationRepo;
            ItemManager = itemManager;
            GameAttributeManager = gameAttributeMangaer;
            ExitAttributeManager = exitAttributeManager;
        }
        /// <summary>
        /// Create an empty new location and connecting exits from a provided location
        /// </summary>
        /// <param name="currentLocationItemId">Existing location item to link to new location with exits</param>
        /// <returns></returns>
        public LocationItem CreateFromHere(Guid currentLocationItemId)
        {
            var currentLocation = ItemRepo.Read(currentLocationItemId);
            var locationAttribute = new LocationAttribute()
            {
                Name = "A New Location",
                Description = "The relatively new construction of this place is apparant everywhere you look. In several places the c# substrate seems to be leaking from above, while behind you a small puddle of sql statements have coalesced into a small puddle. Ew..."
            };
            var defaultGameItemId = GameAttributeManager.GetDefaultGame().Id;
            var locationItem = Create(locationAttribute, defaultGameItemId).GetAsItem<LocationItem>();
            var toExitAttribute = new ExitAttribute()
            {
                Name = "A New Exit",
                Description = "The paint is still wet on this sign!",
                DestinationLocationId = locationAttribute.Id
            };
            ExitAttributeManager.Create(toExitAttribute, currentLocationItemId);
            var froExitAttribute = new ExitAttribute()
            {
                Name = "A Return Path",
                Description = "Escape back to the real world",
                DestinationLocationId = currentLocation.GetAttribute<LocationAttribute>().Id
            };
            ExitAttributeManager.Create(froExitAttribute, locationItem.Id);
            return locationItem;
        }
        /// <summary>
        /// Read item's detailed location
        /// </summary>
        /// <remarks>
        /// If item itself has no location detail then item's parents will be 
        /// traversed until one is found
        /// </remarks>
        /// <param name="item">Item that has attached detail location data</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>The Item's detailed location data. Null if none found</returns>
        new public LocationAttribute Read(Item item, TransactionOptions options = null)
        {
            LocationAttribute location = null;
            Item traverseItem = item;
            while (traverseItem.ParentId != null && !IsManaging(traverseItem))
            {
                traverseItem = ItemRepo.Read(traverseItem.ParentId.Value, options);
            }
            if (IsManaging(traverseItem))
            {
                location = DetailLocationRepo.Read(traverseItem, options);
            }
            return location;
        }
        /// <summary>
        /// Determine if an item has attributes that may be managed
        /// </summary>
        /// <param name="item">Item that may posses attributes</param>
        public bool IsManaging(Item item)
        {
            var managed = false;
            if (DetailLocationRepo.Read(item) != null)
            {
                managed = true;
            }
            return managed;
        }
        /// <summary>
        /// Update attribute's name
        /// </summary>
        /// <param name="id">Unique attribute identifier</param>
        /// <param name="name">Attribute's new name</param>
        /// <returns></returns>
        public LocationAttribute UpdateName(Guid id, string name)
        {
            var locationAttribute = Read(id);
            locationAttribute.Name = name;
            return Update(locationAttribute);
        }
        /// <summary>
        /// Update attribute's description
        /// </summary>
        /// <param name="id">Unique attribute identifier</param>
        /// <param name="description">Attribute's new description</param>
        /// <returns></returns>
        public LocationAttribute UpdateDescription(Guid id, string description)
        {
            var locationAttribute = Read(id);
            locationAttribute.Description = description;
            return Update(locationAttribute);
        }
    }
}
