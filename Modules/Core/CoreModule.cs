using BeforeOurTime.Business.Apis.Items;
using BeforeOurTime.Business.Modules.Core.Dbs.EF;
using BeforeOurTime.Business.Modules.Core.ItemProperties.Visibles;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core;
using BeforeOurTime.Models.Modules.Core.Dbs;
using BeforeOurTime.Models.Modules.Core.Messages.ItemCrud.CreateItem;
using BeforeOurTime.Models.Modules.Core.Messages.ItemCrud.DeleteItem;
using BeforeOurTime.Models.Modules.Core.Messages.ItemCrud.ReadItem;
using BeforeOurTime.Models.Modules.Core.Messages.ItemCrud.UpdateItem;
using BeforeOurTime.Models.Modules.Core.Messages.ItemGraph;
using BeforeOurTime.Models.Modules.Core.Messages.ItemJson.CreateItemJson;
using BeforeOurTime.Models.Modules.Core.Messages.ItemJson.ReadItemJson;
using BeforeOurTime.Models.Modules.Core.Messages.ItemJson.UpdateItemJson;
using BeforeOurTime.Models.Modules.Core.Messages.UseItem;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOutTime.Business.Dbs.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Modules.Core
{
    public partial class CoreModule : ICoreModule
    {
        /// <summary>
        /// Subscribe to be notified when this module and it's managers have been loaded
        /// </summary>
        public event ModuleReadyDelegate ModuleReadyEvent;
        /// <summary>
        /// Entity framework database context
        /// </summary>
        private EFCoreModuleContext Db { set; get; }
        /// <summary>
        /// Manage all modules
        /// </summary>
        private IModuleManager ModuleManager { set; get; }
        /// <summary>
        /// Managers created or required by the module
        /// </summary>
        private List<IModelManager> Managers { set; get; } = new List<IModelManager>();
        /// <summary>
        /// Data repositories created or required by the module
        /// </summary>
        private List<ICrudModelRepository> Repositories { set; get; } = new List<ICrudModelRepository>();
        /// <summary>
        /// Item data repository
        /// </summary>
        private IItemRepo ItemRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public CoreModule(
            IModuleManager moduleManager)
        {
            ModuleManager = moduleManager;
            var connectionString = ModuleManager.GetConfiguration().GetConnectionString("DefaultConnection");
            var dbOptions = new DbContextOptionsBuilder<BaseContext>();
                dbOptions.UseSqlServer(connectionString);
            Db = new EFCoreModuleContext(dbOptions.Options);
            Managers = BuildManagers(ModuleManager, Db);
            Repositories = Managers.SelectMany(x => x.GetRepositories()).ToList();
        }
        /// <summary>
        /// Build all the item managers for the module
        /// </summary>
        /// <param name="db"></param>
        /// <param name="itemRepo"></param>
        /// <returns></returns>
        List<IModelManager> BuildManagers(IModuleManager moduleManager, EFCoreModuleContext db)
        {
            var managers = new List<IModelManager>
            {
                new ItemManager(moduleManager, new ItemRepo(db)),
                new MessageManager(moduleManager, new MessageDataRepo(db)),
                new VisibleItemDataManager(moduleManager, new EFVisibleItemDataRepo(db, moduleManager.GetItemRepo())),
            };
            return managers;
        }
        /// <summary>
        /// Get repositories declared by the module
        /// </summary>
        /// <returns></returns>
        public List<ICrudModelRepository> GetRepositories()
        {
            return Repositories;
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
        /// Get item managers declared by the module
        /// </summary>
        /// <returns></returns>
        public List<IModelManager> GetManagers()
        {
            return Managers;
        }
        /// <summary>
        /// Get manager of specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetManager<T>()
        {
            return Managers.Where(x => x is T).Select(x => (T)x).FirstOrDefault();
        }
        /// <summary>
        /// Get message identifiers of messages handled by module
        /// </summary>
        /// <returns></returns>
        public List<Guid> RegisterForMessages()
        {
            return new List<Guid>()
            {
                CoreReadItemGraphRequest._Id,
                CoreReadItemJsonRequest._Id,
                CoreUpdateItemJsonRequest._Id,
                CoreCreateItemJsonRequest._Id,
                CoreCreateItemCrudRequest._Id,
                CoreReadItemCrudRequest._Id,
                CoreUpdateItemCrudRequest._Id,
                CoreDeleteItemCrudRequest._Id,
                CoreUseItemRequest._Id
            };
        }
        /// <summary>
        /// Initialize module
        /// </summary>
        /// <param name="repositories"></param>
        public void Initialize(List<ICrudModelRepository> repositories)
        {
            ItemRepo = repositories
                .Where(x => x is IItemRepo)
                .Select(x => (IItemRepo)x).FirstOrDefault();
            ModuleManager.GetItemRepo().OnItemCreate += OnItemCreate;
            ModuleManager.GetItemRepo().OnItemRead += OnItemRead;
            ModuleManager.GetItemRepo().OnItemUpdate += OnItemUpdate;
            ModuleManager.GetItemRepo().OnItemDelete += OnItemDelete;
        }
        /// <summary>
        /// Get module's self assigned order. 
        /// </summary>
        /// <remarks>
        /// Lower numbers execute first, therefore a higher module order
        /// allows for previous module loaded values to be altered.
        /// </remarks>
        /// <returns></returns>
        public int GetOrder()
        {
            return 10;
        }
        #region On Item Hooks
        /// <summary>
        /// Create attribute, if present, after item is created
        /// </summary>
        /// <param name="item">Base item just created from datastore</param>
        public void OnItemCreate(Item item)
        {
            Managers.Where(x => x is IItemModelManager).ToList().ForEach(manager =>
            {
                ((IItemModelManager)manager).OnItemCreate(item);
            });
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item just read from datastore</param>
        public void OnItemRead(Item item)
        {
            Managers.Where(x => x is IItemModelManager).ToList().ForEach(manager =>
            {
                ((IItemModelManager)manager).OnItemRead(item);
            });
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item about to be persisted to datastore</param>
        public void OnItemUpdate(Item item)
        {
            Managers.Where(x => x is IItemModelManager).ToList().ForEach(manager =>
            {
                ((IItemModelManager)manager).OnItemUpdate(item);
            });
        }
        /// <summary>
        /// Delete attribute of base item before base item is deleted
        /// </summary>
        /// <param name="item">Base item about to be deleted</param>
        public void OnItemDelete(Item item)
        {
            Managers.Where(x => x is IItemModelManager).ToList().ForEach(manager =>
            {
                ((IItemModelManager)manager).OnItemDelete(item);
            });
        }
        #endregion
        #region Message Handlers
        /// <summary>
        /// Handle a message
        /// </summary>
        /// <param name="api"></param>
        /// <param name="origin">Item that initiated request</param>
        /// <param name="message"></param>
        /// <param name="response"></param>
        public IResponse HandleMessage(
            IMessage message, 
            Item origin,
            IModuleManager moduleManager, 
            IResponse response)
        {
            if (message.GetMessageId() == CoreReadItemGraphRequest._Id)
                response = HandleCoreReadItemGraphRequest(message, origin, moduleManager, response);
            if (message.GetMessageId() == CoreCreateItemCrudRequest._Id)
                response = HandleCoreCreateItemCrudRequest(message, origin, moduleManager, response);
            if (message.GetMessageId() == CoreReadItemCrudRequest._Id)
                response = HandleCoreReadItemCrudRequest(message, origin, moduleManager, response);
            if (message.GetMessageId() == CoreUpdateItemCrudRequest._Id)
                response = HandleCoreUpdateItemCrudRequest(message, origin, moduleManager, response);
            if (message.GetMessageId() == CoreDeleteItemCrudRequest._Id)
                response = HandleCoreDeleteItemCrudRequest(message, origin, moduleManager, response);
            if (message.GetMessageId() == CoreReadItemJsonRequest._Id)
                response = HandleCoreReadItemJsonRequest(message, origin, moduleManager, response);
            if (message.GetMessageId() == CoreUpdateItemJsonRequest._Id)
                response = HandleCoreUpdateItemJsonRequest(message, origin, moduleManager, response);
            if (message.GetMessageId() == CoreCreateItemJsonRequest._Id)
                response = HandleCoreCreateItemJsonRequest(message, origin, moduleManager, response);
            if (message.GetMessageId() == CoreUseItemRequest._Id)
                response = HandleCoreUseItemRequest(message, origin, moduleManager, response);
            return response;
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
        #endregion
    }
}
