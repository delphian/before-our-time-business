using BeforeOurTime.Models.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Models.Modules.Account.Models.Data;
using BeforeOurTime.Models.Modules.Account.Dbs;
using BeforeOurTime.Models.Modules.Account.Models;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Terminals;
using Microsoft.Extensions.Logging;
using BeforeOurTime.Models.Logs;
using BeforeOurTime.Models.Modules.Account.Messages.CreateAccount;
using BeforeOurTime.Models.Modules.Account.Messages.LoginAccount;
using BeforeOurTime.Models.Modules.Account.Messages.LogoutAccount;
using BeforeOurTime.Models.Modules.Account.Managers;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Messages.Requests;

namespace BeforeOurTime.Business.Modules.Account.Managers
{
    public partial class AccountCharacterManager : ModelManager<AccountCharacterData>, IAccountCharacterManager
    {
        /// <summary>
        /// Centralized log messages
        /// </summary>
        private IBotLogger Logger { set; get; }
        private IAccountCharacterDataRepo AccountCharacterDataRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public AccountCharacterManager(
            IBotLogger logger,
            IItemRepo itemRepo,
            IAccountCharacterDataRepo accountCharacterDataRepo)
        {
            Logger = logger;
            AccountCharacterDataRepo = accountCharacterDataRepo;
        }
        /// <summary>
        /// Get all repositories declared by manager
        /// </summary>
        /// <returns></returns>
        public List<ICrudModelRepository> GetRepositories()
        {
            return new List<ICrudModelRepository>() { AccountCharacterDataRepo };
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
        /// Create a new account character
        /// </summary>
        /// <param name="accountId">Unique identifier of account</param>
        /// <param name="password">Unique identifier of item to register as account character</param>
        public AccountCharacterData Create(Guid accountId, Guid characterItemId)
        {
            var accountCharacter = AccountCharacterDataRepo.Create(new List<AccountCharacterData>()
            {
                new AccountCharacterData()
                {
                    AccountId = accountId,
                    CharacterItemId = characterItemId
                }
            }).FirstOrDefault();
            return accountCharacter;
        }
        /// <summary>
        /// Read all account characters for a single account
        /// </summary>
        /// <param name="accountId">Unique account identifier</param>
        /// <returns></returns>
        public List<AccountCharacterData> ReadByAccound(Guid accountId)
        {
            var characterAccounts = AccountCharacterDataRepo.ReadByAccount(new List<Guid>() { accountId });
            return characterAccounts;
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
    }
}