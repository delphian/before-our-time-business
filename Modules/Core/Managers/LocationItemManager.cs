using BeforeOurTime.Models.Items;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Models.Modules.Core.Models.Data;
using BeforeOurTime.Models.Modules.Core.Dbs;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using System.Linq;
using Microsoft.Extensions.Logging;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models;

namespace BeforeOurTime.Business.Modules.Core.Managers
{
    public class LocationItemManager : ItemModelManager<LocationItem>, ILocationItemManager
    {
        /// <summary>
        /// Centralized log messages
        /// </summary>
        private ILogger Logger { set; get; }
        private ILocationDataRepo LocationDataRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public LocationItemManager(
            ILogger logger,
            IItemRepo itemRepo,
            ILocationDataRepo locationDataRepo) : base(itemRepo)
        {
            Logger = logger;
            LocationDataRepo = locationDataRepo;
        }
        /// <summary>
        /// Get all repositories declared by manager
        /// </summary>
        /// <returns></returns>
        public List<ICrudModelRepository> GetRepositories()
        {
            return new List<ICrudModelRepository>() { LocationDataRepo };
        }
        /// <summary>
        /// Get repository as interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetRepository<T>() where T : ICrudModelRepository
        {
            return GetRepositories().Where(x => x is T).Select(x => (T)x).FirstOrDefault();
        }
        /// <summary>
        /// Create an empty new location and connecting exits from a provided location
        /// </summary>
        /// <param name="currentLocationItemId">Existing location item to link to new location with exits</param>
        /// <returns></returns>
        public LocationItem CreateFromHere(Guid currentLocationItemId)
        {
//            var currentLocation = ItemRepo.Read(currentLocationItemId);
//            var locationAttribute = new LocationAttribute()
//            {
//                Name = "A New Location",
//                Description = "The relatively new construction of this place is apparant everywhere you look. In several places the c# substrate seems to be leaking from above, while behind you a small puddle of sql statements have coalesced into a small puddle. Ew..."
//            };
//            var defaultGameItemId = ModuleManager.GetModule<ICoreModule>().GetDefaultGame().Id;
//            var locationItem = Create(locationAttribute, defaultGameItemId).GetAsItem<LocationItem>();
//            var toExitAttribute = new ExitAttribute()
//            {
//                Name = "A New Exit",
//                Description = "The paint is still wet on this sign!",
//                DestinationLocationId = locationAttribute.Id
//            };
//            ExitAttributeManager.Create(toExitAttribute, currentLocationItemId);
//            var froExitAttribute = new ExitAttribute()
//            {
//                Name = "A Return Path",
//                Description = "Escape back to the real world",
//                DestinationLocationId = currentLocation.GetAttribute<LocationAttribute>().Id
//            };
//            ExitAttributeManager.Create(froExitAttribute, locationItem.Id);
//            return locationItem;
return null;
        }
        #region On Item Hooks
        /// <summary>
        /// Create attribute, if present, after item is created
        /// </summary>
        /// <param name="item">Base item just created from datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemCreate(Item item, TransactionOptions options = null)
        {
            if (item.HasData<LocationData>())
            {
                var data = item.GetData<LocationData>();
                data.DataItemId = item.Id;
                LocationDataRepo.Create(data, options);
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item just read from datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemRead(Item item, TransactionOptions options = null)
        {
            var locationData = LocationDataRepo.Read(item, options);
            if (locationData != null)
            {
                item.Data.Add(locationData);
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item about to be persisted to datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemUpdate(Item item, TransactionOptions options = null)
        {
            if (item.HasData<LocationData>())
            {
                var data = item.GetData<LocationData>();
                LocationDataRepo.Update(data, options);
            }
        }
        /// <summary>
        /// Delete attribute of base item before base item is deleted
        /// </summary>
        /// <param name="item">Base item about to be deleted</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemDelete(Item item, TransactionOptions options = null)
        {
            if (item.HasData<LocationData>())
            {
                var data = item.GetData<LocationData>();
                LocationDataRepo.Delete(data, options);
            }
        }
        #endregion
    }
}