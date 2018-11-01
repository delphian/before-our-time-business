using BeforeOurTime.Business.Dbs.EF;
using BeforeOurTime.Models.Modules.Account.Dbs;
using BeforeOurTime.Models.Modules.Terminal.Models.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Modules.Terminal.Dbs.EF
{
    public class EFTerminalDataRepo : Repository<TerminalData>, ITerminalDataRepo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">Entity framework database context</param>
        public EFTerminalDataRepo(EFTerminalModuleContext db) : base(db, db.GetDbSet<TerminalData>())
        {
        }
    }
}
