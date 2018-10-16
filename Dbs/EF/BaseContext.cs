﻿using BeforeOurTime.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
    }
}
