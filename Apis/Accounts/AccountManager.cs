using System.Collections.Generic;
using BeforeOurTime.Repository.Models;
using BeforeOurTime.Repository.Models.Accounts;
using System.Linq;
using System;
using BeforeOurTime.Models.Accounts.Authentication.Providers;
using BeforeOurTime.Models.Accounts;
using BeforeOurTime.Models.Accounts.Authentication;
using BeforeOurTime.Models;

namespace BeforeOurTime.Business.Apis.Accounts
{
    /// <summary>
    /// Main environment interface for accounts
    /// </summary>
    public class AccountManager : IAccountManager
    {
        protected IRepository<AuthenticationBotMeta> AuthenBotMetaRepo { set; get; }
        protected IAccountRepo AccountRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="authenBotMetaRepo"></param>
        /// <param name="accountRepo"></param>
        public AccountManager(
            IAccountRepo accountRepo,
            IRepository<AuthenticationBotMeta> authenBotMetaRepo)
        {
            AuthenBotMetaRepo = authenBotMetaRepo;
            AccountRepo = accountRepo;
        }
        /// <summary>
        /// Create a new account and local authentication credentials
        /// </summary>
        /// <param name="name">Friendly account name</param>
        /// <param name="email">Login email address for account</param>
        /// <param name="password">Password for account</param>
        public Account Create(string name, string email, string password)
        {
            var account = AccountRepo.Create(new List<Account>()
            {
                new Account()
                {
                    Name = name
                }
            }).FirstOrDefault();
            var credentials = AuthenBotMetaRepo.Create(new List<AuthenticationBotMeta>()
            {
                new AuthenticationBotMeta()
                {
                    Email = email,
                    Password = BCrypt.Net.BCrypt.HashPassword(password),
                    AccountId = account.Id
                }
            }).FirstOrDefault();
            return account;
        }
        /// <summary>
        /// Read a single account
        /// </summary>
        /// <param name="id">Unique account identifier</param>
        /// <returns></returns>
        public Account Read(Guid id)
        {
            return AccountRepo.Read(id);
        }
        /// <summary>
        /// Read all accounts, or specify an offset and limit
        /// </summary>
        /// <param name="offset">Number of account records to skip</param>
        /// <param name="limit">Maximum number of account records to return</param>
        /// <returns></returns>
        public List<Account> Read(int? offset = null, int? limit = null)
        {
            return AccountRepo.Read(offset, limit);
        }
        /// <summary>
        /// Authenticate a user name and password
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>User account if authenticated, otherwise null</returns>
        public Account Authenticate(string email, string password)
        {
            var authenRequest = new AuthenticationRequest()
            {
                PrincipalName = email,
                PrincipalPassword = password
            };
            var account = AccountRepo.Read(authenRequest).FirstOrDefault();
            return account;
        }
        /// <summary>
        /// Read a local authentication credential
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public AuthenticationBotMeta ReadCredential(Guid accountId)
        {
            return AuthenBotMetaRepo.Read().Where(x => x.AccountId == accountId).FirstOrDefault();
        }
        /// <summary>
        /// Update a credential's password
        /// </summary>
        /// <param name="credential">Local authentication credential</param>
        /// <param name="password">New password</param>
        /// <returns></returns>
        public AuthenticationBotMeta UpdateCredentialPassword(AuthenticationBotMeta credential, string password)
        {
            credential.Password = BCrypt.Net.BCrypt.HashPassword(password);
            return AuthenBotMetaRepo.Update(new List<AuthenticationBotMeta>() { credential }).FirstOrDefault();
        }
        /// <summary>
        /// Delete a single account
        /// </summary>
        /// <param name="id">Account to delete</param>
        public void Delete(Account account)
        {
            AccountRepo.Delete(account);
        }
    }
}
