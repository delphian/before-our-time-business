using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Models.Modules.Account.Models.Data;
using BeforeOurTime.Models.Modules.Account.Dbs;
using BeforeOurTime.Models.Modules.Account.Models;
using BeforeOurTime.Models.Messages.Responses;
using Microsoft.Extensions.Logging;
using BeforeOurTime.Models.Logs;
using BeforeOurTime.Models.Modules.Account.Managers;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Models.Items;

namespace BeforeOurTime.Business.Modules.Account.Managers
{
    public partial class AccountManager : ModelManager<AccountData>, IAccountManager
    {
        /// <summary>
        /// Manage all modules
        /// </summary>
        private IModuleManager ModuleManager { set; get; }
        /// <summary>
        /// Repository for manager
        /// </summary>
        private IAccountDataRepo AccountDataRepo { set; get; }
        /// <summary>
        /// Character manager
        /// </summary>
        private IAccountCharacterManager CharacterManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public AccountManager(
            IModuleManager moduleManager,
            IAccountDataRepo accountDataRepo)
        {
            ModuleManager = moduleManager;
            AccountDataRepo = accountDataRepo;
        }
        /// <summary>
        /// Get all repositories declared by manager
        /// </summary>
        /// <returns></returns>
        public List<ICrudModelRepository> GetRepositories()
        {
            return new List<ICrudModelRepository>() { AccountDataRepo };
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
        /// Create a new account
        /// </summary>
        /// <param name="name">Login name</param>
        /// <param name="password">Login password</param>
        /// <param name="temporary">Account is temporary for trial purposes</param>
        /// <param name="admin">Account has administrative rights</param>
        public AccountData Create(
            string name,
            string password,
            bool temporary = false,
            bool admin = false)
        {
            var account = AccountDataRepo.Create(new List<AccountData>()
            {
                new AccountData()
                {
                    Name = name,
                    Password = BCrypt.Net.BCrypt.HashPassword(password),
                    Temporary = temporary,
                    Admin = admin
                }
            }).FirstOrDefault();
            return account;
        }
        /// <summary>
        /// Update existing account
        /// </summary>
        /// <param name="accountData">AccountData containing updated properties</param>
        /// <returns></returns>
        public AccountData Update(AccountData accountData)
        {
            accountData.Password = BCrypt.Net.BCrypt.HashPassword(accountData.Password);
            AccountDataRepo.Update(accountData);
            return accountData;
        }
        /// <summary>
        /// Delete an account and all associated account characters
        /// </summary>
        /// <param name="accountId"></param>
        public void Delete(Guid accountId)
        {
            var accountCharacterDatas = ModuleManager.GetManager<IAccountCharacterManager>()
                .ReadByAccount(accountId);
            var accountCharacterItemIds = accountCharacterDatas.Select(x => x.CharacterItemId).ToList();
            if (accountCharacterItemIds.Count > 0)
            {
                ModuleManager.GetManager<IAccountCharacterManager>().Delete(accountCharacterItemIds);
            }
            var accountData = AccountDataRepo.Read(accountId);
            AccountDataRepo.Delete(accountData);
        }
        /// <summary>
        /// Authenticate a user name and password
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>User account if authenticated, otherwise null</returns>
        public AccountData Authenticate(string email, string password)
        {
            var authenRequest = new AuthenticationRequest()
            {
                PrincipalName = email,
                PrincipalPassword = password
            };
            var account = AccountDataRepo.Read(authenRequest);
            return account;
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
            if (item.HasData<AccountData>())
            {
                var data = item.GetData<AccountData>();
                data.DataItemId = item.Id;
                AccountDataRepo.Create(data);
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item just read from datastore</param>
        public void OnItemRead(Item item)
        {
            var characterManager = ModuleManager.GetManager<IAccountCharacterManager>();
            var characterData = characterManager.ReadByCharacter(item.Id);
            if (characterData != null)
            {
                var accountData = AccountDataRepo.Read(characterData.AccountId);
//                accountData.Password = null;
                item.Data.Add(accountData);
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item about to be persisted to datastore</param>
        public void OnItemUpdate(Item item)
        {
            if (item.HasData<AccountData>())
            {
                var data = item.GetData<AccountData>();
                if (data.Id == Guid.Empty)
                {
                    OnItemCreate(item);
                }
                else
                {
                    AccountDataRepo.Update(data);
                }
            }
            //else if (AccountDataRepo.Read(item) is AccountData data)
            //{
            //    AccountDataRepo.Delete(data);
            //}
        }
        /// <summary>
        /// Delete attribute of base item before base item is deleted
        /// </summary>
        /// <param name="item">Base item about to be deleted</param>
        public void OnItemDelete(Item item)
        {
            if (item.HasData<AccountData>())
            {
                var data = item.GetData<AccountData>();
                AccountDataRepo.Delete(data);
            }
        }
        #endregion
    }
}