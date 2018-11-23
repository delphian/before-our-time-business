using BeforeOurTime.Models;
using BeforeOurTime.Models.Exceptions;
using BeforeOurTime.Models.Logs;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Dbs;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models.Modules.Core.Messages.UseItem;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Terminal.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Modules
{
    /// <summary>
    /// Register through reflection and manage all modules
    /// </summary>
    public class ModuleManager : IModuleManager
    {
        /// <summary>
        /// System configuration
        /// </summary>
        private IConfiguration Configuration { set; get; }
        /// <summary>
        /// Centralized logging system
        /// </summary>
        private IBotLogger Logger { set; get; }
        /// <summary>
        /// List of all registered Api Modules
        /// </summary>
        private List<IModule> Modules { set; get; } = new List<IModule>();
        /// <summary>
        /// List of all registered CRUD repositories
        /// </summary>
        private List<ICrudModelRepository> Repositories { set; get; } = new List<ICrudModelRepository>();
        /// <summary>
        /// List of all registered item managers
        /// </summary>
        private List<IModelManager> Managers { set; get; } = new List<IModelManager>();
        /// <summary>
        /// Record which modules have registered for specific messages
        /// </summary>
        private Dictionary<Guid, List<IModule>> MessageHandlers { set; get; } = new Dictionary<Guid, List<IModule>>();
        /// <summary>
        /// Constructor
        /// </summary>
        public ModuleManager(
            IConfiguration configuration, 
            IBotLogger logger)
        {
            Configuration = configuration;
            Logger = logger;
            RegisterModules();
        }
        /// <summary>
        /// Register all API modules that implement IApiModule
        /// </summary>
        private void RegisterModules()
        {
            var interfaceType = typeof(IModule);
            Modules = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x => (IModule)Activator.CreateInstance(x, new object[] { this }))
                .ToList();
            // Ordering modules before letting them register for item CRUD
            // events works in practice, but is not guarenteed. This should
            // be refactored.
            Modules.OrderBy(x => x.GetOrder()).ToList().ForEach((module) =>
            {
                Logger.LogDebug($"Module {module.GetType().Name}: loaded");
                module.GetManagers().ForEach(manager =>
                {
                    Logger.LogDebug($"Module {module.GetType().Name}: loading model manager {manager.GetType().Name}");
                    Managers.Add(manager);
                });
                Repositories.AddRange(module.GetRepositories());
                module.RegisterForMessages().ForEach(messageId =>
                {
                    if (MessageHandlers.ContainsKey(messageId))
                    {
                        MessageHandlers[messageId].Add(module);
                    }
                    else
                    {
                        MessageHandlers.Add(messageId, new List<IModule>() { module });
                    }
                });
            });
            Modules.ForEach((module) =>
            {
                module.Initialize(Repositories);
                Logger.LogInformation($"Module {module.GetType().Name} initialized");
            });
        }
        /// <summary>
        /// Get API module that implements interface
        /// </summary>
        /// <typeparam name="T">Interface that API module must implement</typeparam>
        /// <returns>API module if found, otherwise null</returns>
        public T GetModule<T>() where T : IModule
        {
            var module = Modules.Where(x => x is T).Select(x => x).FirstOrDefault();
            return (T)module;
        }
        /// <summary>
        /// Get repository that implements interface
        /// </summary>
        /// <typeparam name="T">Interface that repository must implement</typeparam>
        /// <returns></returns>
        public T GetRepository<T>() where T : ICrudModelRepository
        {
            var repository = Repositories.Where(x => x is T).Select(x => x).FirstOrDefault();
            return (T)repository;
        }
        /// <summary>
        /// Get item manager that implements interface
        /// </summary>
        /// <typeparam name="T">Interface that item manager must implement</typeparam>
        /// <returns></returns>
        public T GetManager<T>() where T : IModelManager
        {
            var manager = Managers.Where(x => x is T).Select(x => x).FirstOrDefault();
            return (T)manager;
        }
        /// <summary>
        /// Get all managers for an item
        /// </summary>
        /// <param name="item">Item to discover managers of</param>
        /// <returns></returns>
        public IItemModelManager GetManager(Item item)
        {
            var itemManager = Managers
                .Where(x => x is IItemModelManager)
                .Select(x => (IItemModelManager)x)
                .ToList()
                .Where(x => x.IsManaging(item))
                .FirstOrDefault();
            return itemManager;
        }
        /// <summary>
        /// Get all managers for an item type
        /// </summary>
        /// <param name="dataType">Item type that might be managable</param>
        /// <returns></returns>
        public List<IItemModelManager> GetManagers(Type itemType)
        {
            var itemManagers = Managers
                .Where(x => x is IItemModelManager)
                .Select(x => (IItemModelManager)x)
                .ToList()
                .Where(x => x.IsManaging(itemType))
                .ToList();
            return itemManagers;
        }
        /// <summary>
        /// Get all managers for an item data type
        /// </summary>
        /// <param name="dataType">Item data type that might be managable</param>
        /// <returns></returns>
        public List<IItemModelManager> GetManagersOfData(Type dataType)
        {
            var itemManagers = Managers
                .Where(x => x is IItemModelManager)
                .Select(x => (IItemModelManager)x)
                .ToList()
                .Where(x => x.IsManagingData(dataType))
                .ToList();
            return itemManagers;
        }
        /// <summary>
        /// Get all managers for an item property
        /// </summary>
        /// <param name="propertyType">Property type that might be managable</param>
        /// <returns></returns>
        public List<IItemModelManager> GetManagersOfProperty(Type propertyType)
        {
            var itemManagers = Managers
                .Where(x => x is IItemModelManager)
                .Select(x => (IItemModelManager)x)
                .ToList()
                .Where(x => x.IsManagingProperty(propertyType))
                .ToList();
            return itemManagers;
        }
        /// <summary>
        /// Get all modules that have registered handle a message
        /// </summary>
        /// <param name="messageId">Unique message identifier</param>
        /// <returns></returns>
        public List<IModule> GetModulesForMessage(Guid messageId)
        {
            var modules = new List<IModule>();
            if (MessageHandlers.ContainsKey(messageId))
            {
                modules = MessageHandlers[messageId];
            }
            return modules;
        }
        /// <summary>
        /// Handle a message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="origin">Item that initiated request</param>
        /// <param name="response"></param>
        public IResponse HandleMessage(IMessage message, Item origin, IResponse response)
        {
            var modules = GetModulesForMessage(message.GetMessageId());
            modules.ForEach(module =>
            {
                response = module.HandleMessage(message, origin, this, response);
            });
            return response;
        }
        /// <summary>
        /// Execute a use item request
        /// </summary>
        /// <param name="origin">Item that initiated request</param>
        public CoreUseItemResponse UseItem(CoreUseItemRequest request, Item origin, IResponse response)
        {
            response = new CoreUseItemResponse()
            {
                _requestInstanceId = request._requestInstanceId,
                CoreUseItemEvent = new CoreUseItemEvent()
                {
                    Use = request.Use,
                    Using = origin,
                    Success = false
                }
            };
            try
            {
                var item = GetManager<IItemManager>().Read(request.ItemId.Value);
                var manager = GetManager(item);
                if (manager == null)
                {
                    throw new BeforeOurTimeException($"No manager found to use item {item.Id.ToString()}");
                }
                var result = manager.UseItem(request, origin, response);
                ((CoreUseItemResponse)response)._responseSuccess = true;
                ((CoreUseItemResponse)response)._responseMessage = result;
                ((CoreUseItemResponse)response).CoreUseItemEvent.Used = item;
                ((CoreUseItemResponse)response).CoreUseItemEvent.Success = (result == null);
            }
            catch (Exception e)
            {
                ((CoreUseItemResponse)response)._responseSuccess = false;
                ((CoreUseItemResponse)response)._responseMessage = $"Unable to use item: {e.Message}";
            }
            return (CoreUseItemResponse)response;
        }
        /// <summary>
        /// Get configuration
        /// </summary>
        /// <returns></returns>
        public IConfiguration GetConfiguration()
        {
            return Configuration;
        }
        /// <summary>
        /// Get logger
        /// </summary>
        /// <returns></returns>
        public IBotLogger GetLogger()
        {
            return Logger;
        }
        /// <summary>
        /// Get item repository
        /// </summary>
        /// <returns></returns>
        public IItemRepo GetItemRepo()
        {
            return GetRepository<IItemRepo>();
        }
    }
}
