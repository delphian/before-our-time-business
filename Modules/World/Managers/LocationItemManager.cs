using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Logging;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules.World.Models.Items;
using BeforeOurTime.Models.Modules.World.Managers;
using BeforeOurTime.Models.Modules.World.Dbs;
using BeforeOurTime.Models.Modules.World.Models.Data;
using BeforeOurTime.Models.Modules.Core.Models.Data;
using BeforeOurTime.Models.Modules.World;
using BeforeOurTime.Models.Modules.Core.Models.Items;

namespace BeforeOurTime.Business.Modules.World.Managers
{
    public partial class LocationItemManager : ItemModelManager<LocationItem>, ILocationItemManager
    {
        /// <summary>
        /// Manage all modules
        /// </summary>
        private IModuleManager ModuleManager { set; get; }
        /// <summary>
        /// Repository for manager
        /// </summary>
        private ILocationDataRepo LocationDataRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public LocationItemManager(
            IModuleManager moduleManager,
            ILocationDataRepo locationDataRepo)
        {
            ModuleManager = moduleManager;
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
        /// Get all unique item identifiers of managed items
        /// </summary>
        /// <returns></returns>
        public List<Guid> GetItemIds()
        {
            var itemIds = LocationDataRepo.GetItemIds();
            return itemIds;
        }
        /// <summary>
        /// Determine if item data type is managable
        /// </summary>
        /// <param name="propertyData">Item data type that might be managable</param>
        public bool IsManagingData(Type dataType)
        {
            return dataType == typeof(LocationData);
        }
        /// <summary>
        /// Create an empty new location and connecting exits from a provided location
        /// </summary>
        /// <param name="currentLocationItemId">Existing location item to link to new location with exits</param>
        /// <returns></returns>
        public LocationItem CreateFromHere(Guid currentLocationItemId)
        {
            var itemRepo = ModuleManager.GetItemRepo();
            var gameItem = ModuleManager.GetModule<IWorldModule>().GetDefaultGame();
            var currentLocation = itemRepo.Read(currentLocationItemId);
            var newLocationItem = itemRepo.Create(new LocationItem()
            {
                ParentId = gameItem.Id,
                Data = new List<IItemData>()
                {
                    new LocationData()
                    {
                        Name = "A New Location",
                        Description = "Someone has barfed C# code and SQL statements all over the place. It's quite disgusting."
                    }
                },
                Children = new List<Item>()
                {
                    new ExitItem()
                    {
                        Data = new List<IItemData>()
                        {
                            new ExitData()
                            {
                                DestinationLocationId = currentLocationItemId,
                                Name = "A Return Path",
                                Description = "Escape back to the real world"
                            }
                        }
                    }
                }
            }).GetAsItem<LocationItem>();
            currentLocation.Children.Add(new ExitItem()
            {
                ParentId = currentLocation.Id,
                Data =  new List<IItemData>()
                {
                    new ExitData()
                    {
                        DestinationLocationId = newLocationItem.Id,
                        Name = "A New Exit",
                        Description = "The paint is still wet on this sign"
                    }
                }
            });
            itemRepo.Update(currentLocation);
            return newLocationItem;
        }
        /// <summary>
        /// Instantite response object and wrap request handlers in try catch
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IResponse HandleRequestWrapper<T>(
            IRequest request,
            Action<IResponse> callback) where T : Response, new()
        {
            var response = new T()
            {
                _requestInstanceId = request.GetRequestInstanceId(),
            };
            try
            {
                callback(response);
            }
            catch (Exception e)
            {
                ModuleManager.GetLogger().LogException($"While handling {request.GetMessageName()}", e);
                response._responseSuccess = false;
                response._responseMessage = e.Message;
            }
            return response;
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