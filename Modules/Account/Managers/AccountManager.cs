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
        public AccountData Create(string name, string password, bool temporary = false)
        {
            var account = AccountDataRepo.Create(new List<AccountData>()
            {
                new AccountData()
                {
                    Temporary = temporary,
                    Name = name,
                    Password = BCrypt.Net.BCrypt.HashPassword(password)
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
        /// <summary>
        /// Attach terminal data to item
        /// </summary>
        /// <param name="item"></param>
        public void OnItemRead(Item item)
        {
            if (item.Type == ItemType.Character)
            {
                var characterManager = ModuleManager.GetManager<IAccountCharacterManager>();
                var characterData = characterManager.ReadByCharacter(item.Id);
                if (characterData != null)
                {
                    var accountData = AccountDataRepo.Read(characterData.AccountId);
                    item.Data.Add(accountData);
                }
            }
        }
    }
}