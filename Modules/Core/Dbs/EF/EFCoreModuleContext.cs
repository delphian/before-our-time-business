using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Models;
using Microsoft.Extensions.Configuration;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Modules.Core.Models.Data;

namespace BeforeOurTime.Business.Modules.Core.Dbs.EF
{
    /// <summary>
    /// Entity framework database context
    /// </summary>
    public class EFCoreModuleContext : DbContext
    {
        public DbSet<GameData> Games { set; get; }
        public EFCoreModuleContext() : base() { }
        public EFCoreModuleContext(DbContextOptions<EFCoreModuleContext> options) : base(options) { }
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
            modelBuilder.Ignore<Item>();
            // Item Game Data
            modelBuilder.Entity<GameData>()
                .ToTable("Item_Data_Games");
            modelBuilder.Entity<GameData>()
                .HasKey(x => x.Id);
            modelBuilder.Entity<GameData>()
                .Ignore(x => x.DataType);
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
