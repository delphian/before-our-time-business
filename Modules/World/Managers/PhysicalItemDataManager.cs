using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using BeforeOurTime.Models.Logs;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.World.Models.Items;
using BeforeOurTime.Models.Modules.World.Managers;
using BeforeOurTime.Models.Modules.World.Dbs;
using BeforeOurTime.Models.Modules.World.Models.Data;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Core.Messages.UseItem;
using BeforeOurTime.Models.Modules.World.Messages.Location.ReadLocationSummary;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models.Modules.Terminal.Models;
using BeforeOurTime.Models.Modules.Terminal.Managers;
using BeforeOurTime.Models.Modules.Terminal.Models.Data;
using BeforeOurTime.Models.Messages;

namespace BeforeOurTime.Business.Modules.World.Managers
{
    public partial class PhysicalItemDataManager : ItemModelManager<Item>, IPhysicalItemDataManager
    {
        /// <summary>
        /// Manage all modules
        /// </summary>
        private IModuleManager ModuleManager { set; get; }
        /// <summary>
        /// Repository for manager
        /// </summary>
        private IPhysicalItemDataRepo PhysicalItemDataRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public PhysicalItemDataManager(
            IModuleManager moduleManager,
            IPhysicalItemDataRepo physicalItemDataRepo)
        {
            ModuleManager = moduleManager;
            PhysicalItemDataRepo = physicalItemDataRepo;
        }
        /// <summary>
        /// Get all repositories declared by manager
        /// </summary>
        /// <returns></returns>
        public List<ICrudModelRepository> GetRepositories()
        {
            return new List<ICrudModelRepository>() { PhysicalItemDataRepo };
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
            var itemIds = PhysicalItemDataRepo.GetItemIds();
            return itemIds;
        }
        /// <summary>
        /// Determine if a model type should be managed by this manager
        /// </summary>
        /// <param name="modelType"></param>
        /// <returns></returns>
        public override bool IsManaging(Type modelType)
        {
            return false;
        }
        /// <summary>
        /// Determine if an item is managed
        /// </summary>
        /// <param name="item">Item that may have managable data</param>
        public bool IsManaging(Item item)
        {
            return (item.HasData<PhysicalItemData>());
        }
        /// <summary>
        /// Determine if item data type is managable
        /// </summary>
        /// <param name="propertyData">Item data type that might be managable</param>
        public bool IsManagingData(Type dataType)
        {
            return dataType == typeof(PhysicalItemData);
        }
        /// <summary>
        /// Execute a use item request
        /// </summary>
        /// <param name="origin">Item that initiated request</param>
        public string UseItem(CoreUseItemRequest request, Item origin, IResponse response)
        {
            var itemManager = ModuleManager.GetManager<IItemManager>();
            var messageManager = ModuleManager.GetManager<IMessageManager>();
            return "";
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
        public void OnItemCreate(Item item)
        {
            if (item.HasData<PhysicalItemData>())
            {
                var data = item.GetData<PhysicalItemData>();
                data.DataItemId = item.Id;
                PhysicalItemDataRepo.Create(data);
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item just read from datastore</param>
        public void OnItemRead(Item item)
        {
            var ExitData = PhysicalItemDataRepo.Read(item);
            if (ExitData != null)
            {
                item.Data.Add(ExitData);
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item about to be persisted to datastore</param>
        public void OnItemUpdate(Item item)
        {
            if (item.HasData<PhysicalItemData>())
            {
                var data = item.GetData<PhysicalItemData>();
                PhysicalItemDataRepo.Update(data);
            }
        }
        /// <summary>
        /// Delete attribute of base item before base item is deleted
        /// </summary>
        /// <param name="item">Base item about to be deleted</param>
        public void OnItemDelete(Item item)
        {
            if (item.HasData<PhysicalItemData>())
            {
                var data = item.GetData<PhysicalItemData>();
                PhysicalItemDataRepo.Delete(data);
            }
        }
        #endregion
    }
}
