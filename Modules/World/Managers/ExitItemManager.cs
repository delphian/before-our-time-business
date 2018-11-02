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
    public partial class ExitItemManager : ItemModelManager<ExitItem>, IExitItemManager
    {
        /// <summary>
        /// Manage all modules
        /// </summary>
        private IModuleManager ModuleManager { set; get; }
        /// <summary>
        /// Repository for manager
        /// </summary>
        private IExitDataRepo ExitDataRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public ExitItemManager(
            IModuleManager moduleManager,
            IExitDataRepo exitDataRepo)
        {
            ModuleManager = moduleManager;
            ExitDataRepo = exitDataRepo;
        }
        /// <summary>
        /// Get all repositories declared by manager
        /// </summary>
        /// <returns></returns>
        public List<ICrudModelRepository> GetRepositories()
        {
            return new List<ICrudModelRepository>() { ExitDataRepo };
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
            var itemIds = ExitDataRepo.GetItemIds();
            return itemIds;
        }
        /// <summary>
        /// Determine if an item is managed
        /// </summary>
        /// <param name="item">Item that may have managable data</param>
        public bool IsManaging(Item item)
        {
            return (item.Type == ItemType.Exit);
        }
        /// <summary>
        /// Determine if item data type is managable
        /// </summary>
        /// <param name="propertyData">Item data type that might be managable</param>
        public bool IsManagingData(Type dataType)
        {
            return dataType == typeof(ExitData);
        }
        /// <summary>
        /// Read all exits that target the same destination
        /// </summary>
        /// <param name="locationItem"></param>
        /// <returns></returns>
        public List<Item> GetLocationExits(Guid destinationId)
        {
            var exitDatas = ExitDataRepo.ReadDestinationId(destinationId);
            var items = ModuleManager.GetItemRepo().Read(exitDatas.Select(x => x.DataItemId).ToList());
            return items;
        }
        /// <summary>
        /// Execute a use item request
        /// </summary>
        /// <param name="origin">Item that initiated request</param>
        public string UseItem(CoreUseItemRequest request, Item origin, IResponse response)
        {
            var itemManager = ModuleManager.GetManager<IItemManager>();
            var messageManager = ModuleManager.GetManager<IMessageManager>();
            var exit = itemManager.Read(request.ItemId.Value).GetAsItem<ExitItem>();
            var destinationLocation = itemManager.Read(Guid.Parse(exit.Exit.DestinationId));
            itemManager.Move(origin, destinationLocation, exit);
            var locationSummary = ModuleManager.GetManager<ILocationItemManager>()
                .HandleReadLocationSummaryRequest(new WorldReadLocationSummaryRequest()
                {
                }, origin, ModuleManager, response);
            messageManager.SendMessage(new List<IMessage>() { locationSummary }, new List<Item>() { origin });
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
            if (item.HasData<ExitData>())
            {
                var data = item.GetData<ExitData>();
                data.DataItemId = item.Id;
                ExitDataRepo.Create(data);
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item just read from datastore</param>
        public void OnItemRead(Item item)
        {
            var ExitData = ExitDataRepo.Read(item);
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
            if (item.HasData<ExitData>())
            {
                var data = item.GetData<ExitData>();
                ExitDataRepo.Update(data);
            }
        }
        /// <summary>
        /// Delete attribute of base item before base item is deleted
        /// </summary>
        /// <param name="item">Base item about to be deleted</param>
        public void OnItemDelete(Item item)
        {
            if (item.HasData<ExitData>())
            {
                var data = item.GetData<ExitData>();
                ExitDataRepo.Delete(data);
            }
        }
        #endregion
    }
}
