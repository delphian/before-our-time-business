using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Models;
using Microsoft.Extensions.Configuration;
using BeforeOurTime.Models.Primitives.Images;
using BeforeOutTime.Business.Dbs.EF;
using BeforeOurTime.Models.Modules.Core.Models.Items;

namespace BeforeOurTime.Business.Modules.Core.Dbs.EF
{
    /// <summary>
    /// Entity framework database context
    /// </summary>
    public class EFCoreModuleContext : BaseContext
    {
        public DbSet<Item> Items { set; get; }
        public DbSet<Image> Images { set; get; }
        public EFCoreModuleContext() : base() { }
        public EFCoreModuleContext(DbContextOptions<BaseContext> options) : base(options) { }
        /// <summary>
        /// Used when called from dotnet shell commands
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var path = System.IO.Directory.GetCurrentDirectory();
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile($"{path}/appsettings.json");
            var Configuration = configurationBuilder.Build();
            var defaultConnection = Configuration.GetSection("ConnectionStrings").GetValue<string>("DefaultConnection");
            optionsBuilder.UseSqlServer(defaultConnection);
        }
        /// <summary>
        /// Use fluent api to configure tables
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
