using BeforeOurTime.Business.Apis.Logs;
using BeforeOurTime.Business.Modules.Account.Dbs.EF;
using BeforeOurTime.Business.Modules.Account.Managers;
using BeforeOurTime.Business.Modules.Core.Dbs.EF;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Logs;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules.Account.Dbs;
using BeforeOurTime.Models.Modules.Account.Managers;
using BeforeOurTime.Models.Modules.Account.Messages.CreateAccount;
using BeforeOurTime.Models.Modules.Account.Messages.CreateCharacter;
using BeforeOurTime.Models.Modules.Account.Messages.LoginAccount;
using BeforeOurTime.Models.Modules.Account.Messages.LogoutAccount;
using BeforeOurTime.Models.Modules.Account.Messages.ReadCharacter;
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

namespace BeforeOurTime.Business.Modules.Account
{
    public partial class AccountModule : IAccountModule
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
                new AccountCharacterManager(logger, itemRepo, new EFAccountCharacterDataRepo(db))
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
                AccountReadCharacterRequest._Id,
                AccountCreateAccountRequest._Id,
                AccountLoginAccountRequest._Id,
                AccountLogoutAccountRequest._Id
            };
        }
        /// <summary>
        /// Initialize module
        /// </summary>
        /// <param name="repositories"></param>
        public void Initialize(List<ICrudModelRepository> repositories)
        {
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
            if (message.GetMessageId() == AccountCreateAccountRequest._Id)
                response = GetManager<IAccountManager>().HandleCreateAccountRequest(message, api, terminal, response);
            if (message.GetMessageId() == AccountLoginAccountRequest._Id)
                response = GetManager<IAccountManager>().HandleLoginAccountRequest(message, api, terminal, response);
            if (message.GetMessageId() == AccountLogoutAccountRequest._Id)
                response = GetManager<IAccountManager>().HandleLogoutAccountRequest(message, api, terminal, response);
            if (message.GetMessageId() == AccountCreateCharacterRequest._Id)
                response = GetManager<IAccountCharacterManager>().HandleCreateCharacterRequest(message, api, terminal, response);
            if (message.GetMessageId() == AccountReadCharacterRequest._Id)
                response = GetManager<IAccountCharacterManager>().HandleReadCharacterRequest(message, api, terminal, response);
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
                Logger.LogException($"While handling {request.GetMessageName()}", e);
                response._responseMessage = e.Message;
            }
            return response;
        }
        #endregion
    }
}
