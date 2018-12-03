using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Models.Modules.Account.Models.Data;
using BeforeOurTime.Models.Modules.Account.Dbs;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules.Account.Managers;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Models.Data;
using BeforeOurTime.Models.Modules.World;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.World.ItemProperties.Characters;

namespace BeforeOurTime.Business.Modules.Account.Managers
{
    public partial class AccountCharacterManager : ModelManager<AccountCharacterData>, IAccountCharacterManager
    {
        /// <summary>
        /// Manage all modules
        /// </summary>
        private IModuleManager ModuleManager { set; get; }
        /// <summary>
        /// Manager repository
        /// </summary>
        private IAccountCharacterDataRepo AccountCharacterDataRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public AccountCharacterManager(
            IModuleManager moduleManager,
            IAccountCharacterDataRepo accountCharacterDataRepo)
        {
            ModuleManager = moduleManager;
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
        /// <param name="accountId">Unique account identifer to create character for</param>
        /// <param name="name">Name of character</param>
        /// <param name="temporary">Character is for trial purpose only</param>
        /// <returns></returns>
        public Item Create(Guid accountId, string name, bool temporary = false)
        {
            var characterItem = ModuleManager.GetItemRepo().Create(new CharacterItem()
            {
                ParentId = ModuleManager.GetModule<IWorldModule>().GetDefaultLocation().Id,
                Data = new List<IItemData>()
                {
                    new CharacterItemData()
                    {
                        Name = name,
                        Description = "A brave new player",
                        Temporary = temporary
                    }
                }
            });
            Create(accountId, characterItem.Id);
            return characterItem;
        }
        /// <summary>
        /// Create a new account character based on an existing item
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
        public List<AccountCharacterData> ReadByAccount(Guid accountId)
        {
            var characterAccounts = AccountCharacterDataRepo
                .ReadByAccount(new List<Guid>() { accountId });
            return characterAccounts;
        }
        /// <summary>
        /// Read account for a single character
        /// </summary>
        /// <param name="characterId">Unique character (item) identifier</param>
        /// <returns></returns>
        public AccountCharacterData ReadByCharacter(Guid characterId)
        {
            var characterData = AccountCharacterDataRepo
                .ReadByCharacter(new List<Guid>() { characterId }).FirstOrDefault();
            return characterData;
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
                ModuleManager.GetLogger().LogException($"While handling {request.GetMessageName()}", e);
                response._responseMessage = e.Message;
            }
            return response;
        }
    }
}