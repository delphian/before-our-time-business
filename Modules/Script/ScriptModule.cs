using BeforeOurTime.Business.Modules.Script.Dbs.EF;
using BeforeOurTime.Business.Modules.Script.ItemProperties.Javascripts;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Script;
using BeforeOurTime.Models.Modules.Script.ItemProperties.Javascripts;
using BeforeOutTime.Business.Dbs.EF;
using Jint;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Modules.Script
{
    public partial class ScriptModule : IScriptModule
    {
        /// <summary>
        /// Subscribe to be notified when this module and it's managers have been loaded
        /// </summary>
        public event ModuleReadyDelegate ModuleReadyEvent;
        /// <summary>
        /// Entity framework database context
        /// </summary>
        private EFScriptModuleContext Db { set; get; }
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
        public ScriptModule(
            IModuleManager moduleManager)
        {
            ModuleManager = moduleManager;
            var connectionString = ModuleManager.GetConfiguration().GetConnectionString("DefaultConnection");
            var dbOptions = new DbContextOptionsBuilder<BaseContext>();
            dbOptions.UseSqlServer(connectionString);
            Db = new EFScriptModuleContext(dbOptions.Options);
            Managers = BuildManagers(ModuleManager, Db);
            Repositories = Managers.SelectMany(x => x.GetRepositories()).ToList();
            ModuleManager.ModuleManagerReadyEvent += () =>
            {
                ModuleReadyEvent?.Invoke();
            };
        }
        /// <summary>
        /// Build all the item managers for the module
        /// </summary>
        /// <param name="db"></param>
        /// <param name="itemRepo"></param>
        /// <returns></returns>
        List<IModelManager> BuildManagers(IModuleManager moduleManager, EFScriptModuleContext db)
        {
            var itemRepo = moduleManager.GetItemRepo();
            var managers = new List<IModelManager>
            {
                new JavascriptItemDataManager(moduleManager, this, new EFJavascriptItemDataRepo(db, itemRepo)),
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
                ScriptReadJSDefinitionsRequest._Id
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
            return 1000;
        }
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
            if (message.GetMessageId() == ScriptReadJSDefinitionsRequest._Id)
                response = GetManager<IJavascriptItemDataManager>().HandleReadJSDefinitionsRequest(message, origin, mm, response);
            return response;
        }
        #endregion
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
        /// <param name="options">Options to customize how data is transacted from datastore</param>
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
    }
}
