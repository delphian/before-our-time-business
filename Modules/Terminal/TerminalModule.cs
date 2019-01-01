using BeforeOurTime.Business.Modules.Terminal.Dbs.EF;
using BeforeOurTime.Business.Modules.Terminal.Managers;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Terminal;
using BeforeOurTime.Models.Modules.Terminal.Managers;
using BeforeOutTime.Business.Dbs.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeforeOurTime.Business.Modules.Terminal
{
    public partial class TerminalModule : ITerminalModule
    {
        /// <summary>
        /// Subscribe to be notified when this module and it's managers have been loaded
        /// </summary>
        public event ModuleReadyDelegate ModuleReadyEvent;
        /// <summary>
        /// Entity framework database context
        /// </summary>
        private EFTerminalModuleContext Db { set; get; }
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
        /// Constructor
        /// </summary>
        public TerminalModule(
            IModuleManager moduleManager)
        {
            ModuleManager = moduleManager;
            var connectionString = ModuleManager.GetConfiguration().GetConnectionString("DefaultConnection");
            var dbOptions = new DbContextOptionsBuilder<BaseContext>();
                dbOptions.UseSqlServer(connectionString);
            Db = new EFTerminalModuleContext(dbOptions.Options);
            Managers = BuildManagers(ModuleManager, Db);
            Repositories = Managers.SelectMany(x => x.GetRepositories()).ToList();
        }
        /// <summary>
        /// Build all the item managers for the module
        /// </summary>
        /// <param name="db"></param>
        /// <param name="itemRepo"></param>
        /// <returns></returns>
        List<IModelManager> BuildManagers(IModuleManager moduleManager, EFTerminalModuleContext db)
        {
            var managers = new List<IModelManager>
            {
                new TerminalManager(ModuleManager)
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
        /// Get item manager of specified type
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
            };
        }
        /// <summary>
        /// Initialize module
        /// </summary>
        /// <param name="repositories"></param>
        public void Initialize(List<ICrudModelRepository> repositories)
        {
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
            return 200;
        }
        #region On Item Hooks
        /// <summary>
        /// Create attribute, if present, after item is created
        /// </summary>
        /// <param name="item">Base item just created from datastore</param>
        public void OnItemCreate(Item item)
        {
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item just read from datastore</param>
        public void OnItemRead(Item item)
        {
            Managers.Where(x => x is ITerminalManager).ToList().ForEach(manager =>
            {
                ((ITerminalManager)manager).OnItemRead(item);
            });
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item about to be persisted to datastore</param>
        public void OnItemUpdate(Item item)
        {
        }
        /// <summary>
        /// Delete attribute of base item before base item is deleted
        /// </summary>
        /// <param name="item">Base item about to be deleted</param>
        public void OnItemDelete(Item item)
        {
        }
        #endregion
        #region Message Handlers
        /// <summary>
        /// Handle a message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="origin">Item that initiated request</param>
        /// <param name="mm">Module manager</param>
        /// <param name="response"></param>
        public IResponse HandleMessage(
            IMessage message,
            Item origin,
            IModuleManager mm,
            IResponse response)
        {
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
                response._responseSuccess = true;
            }
            catch (Exception e)
            {
                ModuleManager.GetLogger()
                    .LogException($"While handling {request.GetMessageName()}", e);
                response._responseMessage = e.Message;
            }
            return response;
        }
        #endregion
    }
}
