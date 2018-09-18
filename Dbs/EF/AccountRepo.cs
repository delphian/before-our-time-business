using BeforeOurTime.Models;
using BeforeOurTime.Models.Accounts;
using BeforeOurTime.Models.Accounts.Authentication;
using BeforeOurTime.Repository.Models;
using BeforeOurTime.Repository.Models.Accounts;
using BeforeOutTime.Repository.Dbs.EF;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Repository.Dbs.EF
{
    public class AccountRepo : Repository<Account>, IAccountRepo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">Entity framework database context</param>
        public AccountRepo(BaseContext db) : base(db)
        {
        }
        /// <summary>
        /// Read accounts
        /// </summary>
        /// <param name="accountIds">List of unique account identifiers to load</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns></returns>
        public new List<Account> Read(List<Guid> accountIds, TransactionOptions options = null)
        {
            var accounts = new List<Account>();
            accounts = Db.GetDbSet<Account>()
                .Where(x => accountIds.Contains(x.Id))
                .ToList()
            ?? new List<Account>();
            return accounts;
        }
        /// <summary>
        /// Read all accounts, or specify an offset and limit
        /// </summary>
        /// <param name="offset">Number of records to skip</param>
        /// <param name="limit">Maximum number of records to return</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns></returns>
        public new List<Account> Read(
            int? offset = null, 
            int? limit = null, 
            TransactionOptions options = null)
        {
            var models = new List<Account>();
            IQueryable<Account> modelQuery = Db.GetDbSet<Account>();
            if (offset != null)
            {
                modelQuery = modelQuery.Skip(offset.Value);
            }
            if (limit != null)
            {
                modelQuery = modelQuery.Take(limit.Value);
            }
            models.AddRange(
                modelQuery
                    .ToList());
            return models;
        }
        /// <summary>
        /// Read accounts by login credentials
        /// </summary>
        /// <param name="request">User credentials that may be used for an authentication request</param>
        /// <returns></returns>
        public List<Account> Read(AuthenticationRequest request)
        {
            var accounts = new List<Account>();
            var account = Db.GetDbSet<Account>()
                .Where(x => x.Email == request.PrincipalName)
                .FirstOrDefault();
            if (account != null)
            {
                if (BCrypt.Net.BCrypt.Verify(request.PrincipalPassword, account.Password))
                {
                    accounts.Add(Read(account.Id));
                }
            }
            return accounts;
        }
    }
}
