using System.Collections.Generic;
using BeforeOurTime.Repository.Models;
using BeforeOurTime.Repository.Models.Accounts;
using System.Linq;
using BeforeOurTime.Repository.Models.Accounts.Authentication.Providers;
using BeforeOurTime.Repository.Models.Accounts.Authentication;
using System;

namespace BeforeOurTime.Business.Apis.Accounts
{
    /// <summary>
    /// Main environment interface for accounts
    /// </summary>
    public class AccountManager : IAccountManager
    {
        protected IRepository<AuthenticationBotMeta> AuthenBotMetaRepo { set; get; }
        protected IAuthorGroupRepo AuthorGroupRepo { set; get; }
        protected IAccountRepo AccountRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="authenBotMetaRepo"></param>
        /// <param name="authorGroupRepo"></param>
        /// <param name="accountRepo"></param>
        public AccountManager(
            IAccountRepo accountRepo,
            IAuthorGroupRepo authorGroupRepo,
            IRepository<AuthenticationBotMeta> authenBotMetaRepo)
        {
            AuthenBotMetaRepo = authenBotMetaRepo;
            AuthorGroupRepo = authorGroupRepo;
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
        /// Authenticate a user name and password
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>User account if authenticated, otherwise null</returns>
        public Account Authenticate(string email, string password)
        {
            var account = AccountRepo.Read(
                new AuthenticationRequest() {
                    PrincipalName = email,
                    PrincipalPassword = BCrypt.Net.BCrypt.HashPassword(password)
                })
                .FirstOrDefault();
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
    }
}
