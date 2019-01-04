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
using BeforeOurTime.Models.Modules.World.ItemProperties.Physicals;
using BeforeOurTime.Models.Modules.Core.Models.Properties;
using BeforeOurTime.Models.Modules.Core.Messages.UseItem;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models.Modules.World.ItemProperties.Characters;

namespace BeforeOurTime.Business.Modules.World.ItemProperties.Physicals
{
    public partial class PhysicalItemDataManager : ItemModelManager<Item>, IPhysicalItemDataManager
    {
        private static Guid CommandTake = new Guid("7a878b4a-47cd-461c-8acb-942afa745d3c");
        private static Guid CommandDrop = new Guid("b94b98dd-f089-4c32-b1bf-4d11dfe5e0d9");
        /// <summary>
        /// Manage all modules
        /// </summary>
        private IModuleManager ModuleManager { set; get; }
        /// <summary>
        /// Item manager
        /// </summary>
        private IItemManager ItemManager { set; get; }
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
            ModuleManager.RegisterForItemCommands(HandleUseItemCommand);
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
        /// Handle request to invoke an item command
        /// </summary>
        /// <param name="itemCommand"></param>
        /// <param name="origin"></param>
        public CoreUseItemEvent HandleUseItemCommand(ItemCommand itemCommand, Item origin)
        {
            CoreUseItemEvent continueIfNull = null;
            if (itemCommand.Id == CommandTake)
            {
                var itemManager = ModuleManager.GetManager<IItemManager>();
                var messageManager = ModuleManager.GetManager<IMessageManager>();
                var item = itemManager.Read(itemCommand.ItemId.Value);
                itemManager.Move(item, origin, origin);
                continueIfNull = new CoreUseItemEvent()
                {
                    Success = true,
                    Used = item,
                    Using = origin,
                    Use = itemCommand
                };
            }
            if (itemCommand.Id == CommandDrop)
            {
                var itemManager = ModuleManager.GetManager<IItemManager>();
                var messageManager = ModuleManager.GetManager<IMessageManager>();
                var item = itemManager.Read(itemCommand.ItemId.Value);
                var location = itemManager.Read(origin.ParentId.Value);
                itemManager.Move(item, location, origin);
                continueIfNull = new CoreUseItemEvent()
                {
                    Success = true,
                    Used = item,
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
            var data = PhysicalItemDataRepo.Read(item);
            if (data != null)
            {
                item.Data.Add(data);
                item.AddProperty(typeof(PhysicalItemProperty), new PhysicalItemProperty()
                {
                    Mobile = data.Mobile
                });
                if (data.Mobile == true)
                {
                    if (!item.HasProperty<CommandItemProperty>())
                    {
                        item.AddProperty(typeof(CommandItemProperty), new CommandItemProperty() { Commands = new List<ItemCommand>() });
                    }
                    bool inInventory = (item.ParentId != null) ?
                        ModuleManager.GetRepository<ICharacterItemDataRepo>().ReadItemId(item.ParentId.Value) != null :
                        false;
                    item.GetProperty<CommandItemProperty>().Commands.Add(
                        new ItemCommand()
                        {
                            ItemId = item.Id,
                            Id = (inInventory) ? CommandDrop : CommandTake,
                            Name = (inInventory) ? "Drop" : "Take"
                        });
                }
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
                if (data.Id == Guid.Empty)
                {
                    OnItemCreate(item);
                }
                else
                {
                    PhysicalItemDataRepo.Update(data);
                }
            }
            else if (PhysicalItemDataRepo.Read(item) is PhysicalItemData data)
            {
                PhysicalItemDataRepo.Delete(data);
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
