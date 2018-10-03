using BeforeOurTime.Business.Modules.Account.Dbs.EF;
using BeforeOurTime.Business.Modules.Core.Dbs.EF;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Logs;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Requests.Create;
using BeforeOurTime.Models.Messages.Requests.Login;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules.Account.Dbs;
using BeforeOurTime.Models.Modules.Account.Managers;
using BeforeOurTime.Models.Modules.Core;
using BeforeOurTime.Models.Terminals;
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
    public class AccountModule : IAccountModule
    {
        /// <summary>
        /// Entity framework database context
        /// </summary>
        private EFAccountModuleContext Db { set; get; }
        /// <summary>
        /// System configuration
        /// </summary>
        private IConfiguration Configuration { set; get; }
        /// <summary>
        /// Centralized log messages
        /// </summary>
        private IBotLogger Logger { set; get; }
        /// <summary>
        /// Access to items in the data store
        /// </summary>
        private IItemRepo ItemRepo { set; get; }
        /// <summary>
        /// Managers created or required by the module
        /// </summary>
        private List<IModelManager> Managers { set; get; } = new List<IModelManager>();
        /// <summary>
        /// Data repositories created or required by the module
        /// </summary>
        private List<ICrudModelRepository> Repositories { set; get; } = new List<ICrudModelRepository>();
        /// <summary>
        /// Account data repository
        /// </summary>
        private IAccountDataRepo AccountDataRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="itemRepo">Access to items in the data store</param>
        public AccountModule(
            IConfiguration configuration,
            IBotLogger logger,
            IItemRepo itemRepo)
        {
            Configuration = configuration;
            Logger = logger;
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            var dbOptions = new DbContextOptionsBuilder<EFAccountModuleContext>();
                dbOptions.UseSqlServer(connectionString);
                dbOptions.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            Db = new EFAccountModuleContext(dbOptions.Options);
            ItemRepo = itemRepo;
            Managers = BuildManagers(Logger, Db, ItemRepo);
            Repositories = Managers.SelectMany(x => x.GetRepositories()).ToList();
        }
        /// <summary>
        /// Build all the item managers for the module
        /// </summary>
        /// <param name="db"></param>
        /// <param name="itemRepo"></param>
        /// <returns></returns>
        List<IModelManager> BuildManagers(IBotLogger logger, EFAccountModuleContext db, IItemRepo itemRepo)
        {
            var managers = new List<IModelManager>
            {
                new AccountManager(logger, itemRepo, new EFAccountDataRepo(db)),
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
                CreateAccountRequest._Id,
                LoginRequest._Id,
                LogoutRequest._Id
            };
        }
        /// <summary>
        /// Initialize module
        /// </summary>
        /// <param name="repositories"></param>
        public void Initialize(List<ICrudModelRepository> repositories)
        {
            AccountDataRepo = repositories
                .Where(x => x is IAccountDataRepo)
                .Select(x => (IAccountDataRepo)x).FirstOrDefault();
        }
        #region On Item Hooks
        /// <summary>
        /// Create attribute, if present, after item is created
        /// </summary>
        /// <param name="item">Base item just created from datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemCreate(Item item, TransactionOptions options = null)
        {
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item just read from datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemRead(Item item, TransactionOptions options = null)
        {
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item about to be persisted to datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemUpdate(Item item, TransactionOptions options = null)
        {
        }
        /// <summary>
        /// Delete attribute of base item before base item is deleted
        /// </summary>
        /// <param name="item">Base item about to be deleted</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemDelete(Item item, TransactionOptions options = null)
        {
        }
        #endregion
        #region Message Handlers
        /// <summary>
        /// Handle a message
        /// </summary>
        /// <param name="api"></param>
        /// <param name="message"></param>
        /// <param name="terminal"></param>
        /// <param name="response"></param>
        public IResponse HandleMessage(
            IMessage message, 
            IApi api, 
            ITerminal terminal, 
            IResponse response)
        {
            if (message.GetMessageId() == CreateAccountRequest._Id)
                response = GetManager<IAccountManager>()
                    .HandleCreateAccountRequest(message, api, terminal, response);
            if (message.GetMessageId() == LoginRequest._Id)
                response = GetManager<IAccountManager>()
                    .HandleLoginAccountRequest(message, api, terminal, response);
            if (message.GetMessageId() == LogoutRequest._Id)
                response = GetManager<IAccountManager>()
                    .HandleLogoutAccountRequest(message, api, terminal, response);
            return response;
        }
        #endregion
    }
}
