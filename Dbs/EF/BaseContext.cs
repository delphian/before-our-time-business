using BeforeOurTime.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOutTime.Business.Dbs.EF
{
    /// <summary>
    /// Entity framework database context
    /// </summary>
    public class BaseContext : DbContext
    {
        public BaseContext() : base() { }
        public BaseContext(DbContextOptions<BaseContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
        public DbSet<T> GetDbSet<T>() where T : Model
        {
            var dbSet = Set<T>();
            return dbSet;
        }
        public override void Dispose()
        {
            base.Dispose();
        }
        public override int SaveChanges()
        {
            var updated = base.SaveChanges();
            var entries = ChangeTracker.Entries().Count();
            while (ChangeTracker.Entries().Count() > 0)
            {
                ChangeTracker.Entries().First().State = EntityState.Detached;
            }
            return updated;
        }
    }
}
