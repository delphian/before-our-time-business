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
    public class EFAccountCharacterDataRepo : Repository<AccountCharacterData>, IAccountCharacterDataRepo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">Entity framework database context</param>
        public EFAccountCharacterDataRepo(
            EFAccountModuleContext db) : base(db, db.GetDbSet<AccountCharacterData>())
        {
        }
        /// <summary>
        /// Read all account character item identifiers associated with an account 
        /// </summary>
        /// <param name="accountIds"></param>
        /// <returns></returns>
        public List<AccountCharacterData> ReadByAccount(List<Guid> accountIds)
        {
            var ids = Set.Where(x => accountIds.Contains(x.AccountId)).Select(x => x.Id).ToList();
            return Read(ids);
        }
    }
}
