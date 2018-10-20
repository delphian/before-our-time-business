using BeforeOurTime.Business.Dbs.EF;
using BeforeOurTime.Business.Modules.Core.Dbs.EF;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Modules.Account.Dbs;
using BeforeOurTime.Models.Modules.Account.Models;
using BeforeOurTime.Models.Modules.Account.Models.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Modules.Account.Dbs.EF
{
    public class EFAccountDataRepo : Repository<AccountData>, IAccountDataRepo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">Entity framework database context</param>
        public EFAccountDataRepo(EFAccountModuleContext db) : base(db, db.GetDbSet<AccountData>())
        {
        }
        /// <summary>
        /// Read accounts by login credentials
        /// </summary>
        /// <param name="request">User credentials that may be used for an authentication request</param>
        /// <returns></returns>
        public AccountData Read(AuthenticationRequest request)
        {
            var accounts = new List<AccountData>();
            var account = Db.GetDbSet<AccountData>()
                .Where(x => x.Name == request.PrincipalName)
                .FirstOrDefault();
            if (account != null)
            {
                if (BCrypt.Net.BCrypt.Verify(request.PrincipalPassword, account.Password))
                {
                    accounts.Add(Read(account.Id));
                }
            }
            return accounts.FirstOrDefault();
        }
    }
}
