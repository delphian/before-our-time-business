using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Core.Messages.UseItem;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Modules.Core.Models.Properties;
using BeforeOurTime.Models.Modules.World.ItemProperties.Locations;
using BeforeOurTime.Models.Modules.World.ItemProperties.Locations.Messages.ReadLocationSummary;
using BeforeOurTime.Models.Modules.World.ItemProperties.Exits;

namespace BeforeOurTime.Business.Modules.World.ItemProperties.Exits
{
    public partial class ExitItemDataManager : ItemModelManager<ExitItem>, IExitItemDataManager
    {
        /// <summary>
        /// Manage all modules
        /// </summary>
        private IModuleManager ModuleManager { set; get; }
        /// <summary>
        /// Repository for manager
        /// </summary>
        private IExitItemDataRepo ExitDataRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public ExitItemDataManager(
            IModuleManager moduleManager,
            IExitItemDataRepo exitDataRepo)
        {
            ModuleManager = moduleManager;
            ExitDataRepo = exitDataRepo;
            ModuleManager.RegisterForItemCommands(HandleUseItemCommand);
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
            return dataType == typeof(ExitItemData);
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
        /// Handle request to invoke an item command
        /// </summary>
        /// <param name="itemCommand"></param>
        /// <param name="origin"></param>
        public CoreUseItemEvent HandleUseItemCommand(ItemCommand itemCommand, Item origin)
        {
            CoreUseItemEvent continueIfNull = null;
            if (itemCommand.Id == new Guid("c558c1f9-7d01-45f3-bc35-dcab52b5a37c"))
            {
                var itemManager = ModuleManager.GetManager<IItemManager>();
                var messageManager = ModuleManager.GetManager<IMessageManager>();
                var exitItem = itemManager.Read(itemCommand.ItemId.Value);
                var destinationItem = itemManager.Read(Guid.Parse(exitItem.GetProperty<ExitItemProperty>().DestinationId));
                itemManager.Move(origin, destinationItem, exitItem);
                IResponse mockResponse = new Response();
                var locationSummary = ModuleManager.GetManager<ILocationItemDataManager>()
                    .HandleReadLocationSummaryRequest(new WorldReadLocationSummaryRequest()
                    {
                    }, origin, ModuleManager, mockResponse);
                messageManager.SendMessage(new List<IMessage>() { locationSummary }, new List<Item>() { origin });
                continueIfNull = new CoreUseItemEvent()
                {
                    Success = true,
                    Used = exitItem,
                    Using = origin,
                    Use = itemCommand
                };
            }
            return continueIfNull;
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
            if (item.HasData<ExitItemData>())
            {
                var data = item.GetData<ExitItemData>();
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
            var data = ExitDataRepo.Read(item);
            if (data != null)
            {
                item.Data.Add(data);
                item.AddProperty(typeof(ExitItemProperty), new ExitItemProperty()
                {
                    DestinationId = data.DestinationLocationId.ToString(),
                    Effort = data.Effort,
                    Time = data.Time
                });
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item about to be persisted to datastore</param>
        public void OnItemUpdate(Item item)
        {
            if (item.HasData<ExitItemData>())
            {
                var data = item.GetData<ExitItemData>();
                if (data.Id == Guid.Empty)
                {
                    OnItemCreate(item);
                }
                else
                {
                    ExitDataRepo.Update(data);
                }
            }
        }
        /// <summary>
        /// Delete attribute of base item before base item is deleted
        /// </summary>
        /// <param name="item">Base item about to be deleted</param>
        public void OnItemDelete(Item item)
        {
            if (item.HasData<ExitItemData>())
            {
                var data = item.GetData<ExitItemData>();
                ExitDataRepo.Delete(data);
            }
        }
        #endregion
    }
}
