using BeforeOurTime.Models;
using BeforeOurTime.Models.Items;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Models.ItemAttributes;
using BeforeOurTime.Models.Primitives.Images;
using BeforeOurTime.Models.Modules.Account.Models.Data;
using BeforeOurTime.Models.Modules.Core.Models.Data;
using BeforeOurTime.Models.Modules.World.Models.Data;

namespace BeforeOutTime.Repository.Dbs.EF
{
    /// <summary>
    /// Entity framework database context
    /// </summary>
    public class BaseContext : DbContext
    {
        // Items
        public DbSet<Item> Items { set; get; }

        public BaseContext() : base() { }
        public BaseContext(DbContextOptions<BaseContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<AccountData>();
            modelBuilder.Ignore<LocationData>();
            modelBuilder.Ignore<ExitData>();
            modelBuilder.Ignore<ItemAttribute>();
            // Icon
            modelBuilder.Entity<Image>()
                .ToTable("Icons");
            // Item
            modelBuilder.Entity<Item>()
                .ToTable("Items");
            modelBuilder.Entity<Item>()
                .HasKey(item => item.Id);
            modelBuilder.Entity<Item>()
                .HasMany(item => item.Children)
                .WithOne(item => item.Parent)
                .HasForeignKey(item => item.ParentId);
            modelBuilder.Entity<Item>()
                .Ignore(item => item.ChildrenIds)
                .Ignore(item => item.Attributes)
                .Ignore(item => item.Data);
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
