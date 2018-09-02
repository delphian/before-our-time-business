using BeforeOurTime.Models.Accounts;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Accounts
{
    public interface IAccountManager
    {
        /// <summary>
        /// Create a new account and local authentication credentials
        /// </summary>
        /// <param name="name">Friendly account name</param>
        /// <param name="email">Login email address for account</param>
        /// <param name="password">Password for account</param>
        Account Create(string name, string email, string password);
        /// <summary>
        /// Read a single model
        /// </summary>
        /// <param name="id">Unique model identifier</param>
        /// <returns></returns>
        Account Read(Guid id);
        /// <summary>
        /// Read all accounts, or specify an offset and limit
        /// </summary>
        /// <param name="offset">Number of account records to skip</param>
        /// <param name="limit">Maximum number of account records to return</param>
        /// <returns></returns>
        List<Account> Read(int? offset = null, int? limit = null);
        /// <summary>
        /// Authenticate a user name and password
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>User account if authenticated, otherwise null</returns>
        Account Authenticate(string email, string password);
        /// <summary>
        /// Delete a single account
        /// </summary>
        /// <param name="id">Account to delete</param>
        void Delete(Account account);
    }
}
