using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Models;
using Microsoft.Extensions.Configuration;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Modules.Core.Models.Data;
using BeforeOurTime.Models.Modules.Account.Models.Data;

namespace BeforeOurTime.Business.Modules.Core.Dbs.EF
{
    /// <summary>
    /// Entity framework database context
    /// </summary>
    public class EFCoreModuleContext : DbContext
    {
        public DbSet<GameData> Games { set; get; }
        public DbSet<LocationData> Locations { set; get; }
        public DbSet<CharacterData> Characters { set; get; }
        public DbSet<ExitData> Exits { set; get; }
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
            modelBuilder.Ignore<AccountData>();
            // Item Game Data
            modelBuilder.Entity<GameData>()
                .ToTable("Item_Data_Games");
            modelBuilder.Entity<GameData>()
                .HasKey(x => x.Id);
            modelBuilder.Entity<GameData>()
                .Ignore(x => x.DataType);
            // Item Location Data
            modelBuilder.Entity<LocationData>()
                .ToTable("Item_Data_Locations");
            modelBuilder.Entity<LocationData>()
                .HasKey(x => x.Id);
            modelBuilder.Entity<LocationData>()
                .Ignore(x => x.DataType)
                .Property(x => x.Name).IsRequired();
            // Item Location Data
            modelBuilder.Entity<CharacterData>()
                .ToTable("Item_Data_Characters");
            modelBuilder.Entity<CharacterData>()
                .HasKey(x => x.Id);
            modelBuilder.Entity<CharacterData>()
                .Ignore(x => x.DataType)
                .Property(x => x.Name).IsRequired();
            // Item Exit Data
            modelBuilder.Entity<ExitData>()
                .ToTable("Item_Data_Exits");
            modelBuilder.Entity<ExitData>()
                .HasKey(x => x.Id);
            modelBuilder.Entity<ExitData>()
                .Ignore(x => x.DataType);
            modelBuilder.Entity<ExitData>()
                .Property(x => x.Name).IsRequired();
            modelBuilder.Entity<ExitData>()
                .Property(x => x.DestinationLocationId).IsRequired();
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
