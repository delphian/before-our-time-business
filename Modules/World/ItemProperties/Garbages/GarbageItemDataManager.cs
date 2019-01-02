using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Core.Models.Properties;
using BeforeOurTime.Models.Modules.Core.Messages.UseItem;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models.Modules.World.ItemProperties.Generators;
using BeforeOurTime.Models.Modules.World.ItemProperties.Garbages;
using BeforeOurTime.Models.Logs;
using BeforeOurTime.Models.Modules.Core.Messages.MoveItem;
using BeforeOurTime.Models.Modules.World;
using BeforeOurTime.Models.Messages.Events;
using BeforeOurTime.Models.Modules.Core.ItemProperties.Visibles;
using BeforeOurTime.Models.Modules.World.ItemProperties.Locations;
using Microsoft.Extensions.Configuration;

namespace BeforeOurTime.Business.Modules.World.ItemProperties.Garbages
{
    public partial class GarbageItemDataManager : ItemModelManager<Item>, IGarbageItemDataManager
    {
        /// <summary>
        /// Manage all modules
        /// </summary>
        private IModuleManager ModuleManager { set; get; }
        /// <summary>
        /// Module manager in charge of this data manager
        /// </summary>
        private IWorldModule MyModule { set; get; }
        /// <summary>
        /// Item manager
        /// </summary>
        private IItemManager ItemManager { set; get; }
        /// <summary>
        /// Error logger
        /// </summary>
        private IBotLogger Logger { set; get; }
        /// <summary>
        /// Repository for manager
        /// </summary>
        private IGarbageItemDataRepo GarbageItemDataRepo { set; get; }
        /// <summary>
        /// Tick interval at which garbage collection will execute
        /// </summary>
        private int TickInterval { set; get; }
        /// <summary>
        /// Current tick interval on it's way to the maximum;
        /// </summary>
        private int TickCount { set; get; } = 0;
        /// <summary>
        /// Number of miliseconds between each tick
        /// </summary>
        private int TickTime { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public GarbageItemDataManager(
            IModuleManager moduleManager,
            IWorldModule myModule,
            IGarbageItemDataRepo garbageItemDataRepo)
        {
            ModuleManager = moduleManager;
            MyModule = myModule;
            GarbageItemDataRepo = garbageItemDataRepo;
            Logger = ModuleManager.GetLogger();
            TickTime = moduleManager.GetConfiguration()
                .GetSection("Timing")
                .GetValue<int>("Tick");
            TickInterval = moduleManager.GetConfiguration()
                .GetSection("Modules")
                .GetSection("World")
                .GetSection("Managers")
                .GetSection("Garbage")
                .GetValue<int>("TickInterval");
            ModuleManager.Ticks += OnTick;
            MyModule.ModuleReadyEvent += () =>
            {
                ModuleManager.GetManager<IItemManager>().ItemMoveEvent += OnMoveItemEvent;
                ItemManager = moduleManager.GetManager<IItemManager>();
            };
        }
        /// <summary>
        /// Determine if a moved item requires a garbage timer to be set
        /// </summary>
        /// <param name="itemEvent"></param>
        public void OnMoveItemEvent(IEvent itemEvent)
        {
            var moveItemEvent = (CoreMoveItemEvent)itemEvent;
            if (moveItemEvent.Item.GetData<GarbageItemData>() is GarbageItemData garbage)
            {
                if (moveItemEvent.NewParent != null && moveItemEvent.NewParent.HasProperty<LocationItemProperty>())
                {
                    garbage.IntervalTime = DateTime.Now.AddMilliseconds(TickTime * garbage.Interval);
                    GarbageItemDataRepo.Update(garbage);
                }
            }
        }
        /// <summary>
        /// Handle recurring regular tasks
        /// </summary>
        public void OnTick()
        {
            if (TickCount++ >= TickInterval)
            {
                Logger.LogInformation("Running garbage collection");
                TickCount = 0;
                var garbageItemDatas = GarbageItemDataRepo.ReadExpired();
                if (garbageItemDatas.Count > 0)
                {
                    garbageItemDatas.ForEach((garbageItemData) =>
                    {
                        try
                        {
                            var item = ItemManager.Read(garbageItemData.DataItemId);
                            Logger.LogDebug($"Garbage collecting: {item.GetProperty<VisibleItemProperty>()?.Name ?? item.Id.ToString()}");
                            ItemManager.Delete(new List<Item>() { item });
                        }
                        catch (Exception e)
                        {
                            ModuleManager.GetLogger().LogException("Failed to garbage collect item", e);
                        }
                    });
                }
            }
        }
        /// <summary>
        /// Get all repositories declared by manager
        /// </summary>
        /// <returns></returns>
        public List<ICrudModelRepository> GetRepositories()
        {
            return new List<ICrudModelRepository>() { GarbageItemDataRepo };
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
            var itemIds = GarbageItemDataRepo.GetItemIds();
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
            return (item.HasData<GeneratorItemData>());
        }
        /// <summary>
        /// Determine if item data type is managable
        /// </summary>
        /// <param name="propertyData">Item data type that might be managable</param>
        public bool IsManagingData(Type dataType)
        {
            return dataType == typeof(GeneratorItemData);
        }
        /// <summary>
        /// Handle request to invoke an item command
        /// </summary>
        /// <param name="itemCommand"></param>
        /// <param name="origin"></param>
        public CoreUseItemEvent HandleUseItemCommand(ItemCommand itemCommand, Item origin)
        {
            CoreUseItemEvent continueIfNull = null;
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
            if (item.HasData<GarbageItemData>())
            {
                var data = item.GetData<GarbageItemData>();
                data.DataItemId = item.Id;
                GarbageItemDataRepo.Create(data);
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item just read from datastore</param>
        public void OnItemRead(Item item)
        {
            var data = GarbageItemDataRepo.Read(item);
            if (data != null)
            {
                item.Data.Add(data);
                item.AddProperty(typeof(GarbageItemProperty), new GarbageItemProperty()
                {
                });
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item about to be persisted to datastore</param>
        public void OnItemUpdate(Item item)
        {
            if (item.HasData<GarbageItemData>())
            {
                var data = item.GetData<GarbageItemData>();
                if (data.Id == Guid.Empty)
                {
                    OnItemCreate(item);
                }
                else
                {
                    GarbageItemDataRepo.Update(data);
                }
            }
        }
        /// <summary>
        /// Delete attribute of base item before base item is deleted
        /// </summary>
        /// <param name="item">Base item about to be deleted</param>
        public void OnItemDelete(Item item)
        {
            if (item.HasData<GarbageItemData>())
            {
                var data = item.GetData<GarbageItemData>();
                GarbageItemDataRepo.Delete(data);
            }
        }
        #endregion
    }
}
