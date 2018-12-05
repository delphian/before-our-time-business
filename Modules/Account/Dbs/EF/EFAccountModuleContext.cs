using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Models;
using Microsoft.Extensions.Configuration;
using BeforeOurTime.Models.Modules.Account.Models.Data;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOutTime.Business.Dbs.EF;
using BeforeOurTime.Models.Modules.World.ItemProperties.Locations;

namespace BeforeOurTime.Business.Modules.Core.Dbs.EF
{
    /// <summary>
    /// Entity framework database context
    /// </summary>
    public class EFAccountModuleContext : BaseContext
    {
        public DbSet<AccountData> Accounts { set; get; }
        public DbSet<AccountCharacterData> AccountCharacters { set; get; }
        public EFAccountModuleContext() : base() { }
        public EFAccountModuleContext(DbContextOptions<BaseContext> options) : base(options) { }
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
            modelBuilder.Ignore<LocationItemData>();
            // Account
            modelBuilder.Entity<AccountData>()
                .ToTable("Accounts")
                .HasIndex(account => account.Name).IsUnique(true);
            modelBuilder.Entity<AccountData>()
                .HasKey(account => account.Id);
            modelBuilder.Entity<AccountData>()
                .HasMany<AccountCharacterData>(x => x.Characters)
                    .WithOne(x => x.Account)
                        .HasForeignKey(x => x.AccountId);
            modelBuilder.Entity<AccountData>()
                .Property(x => x.Name).IsRequired();
            // Account Characters
            modelBuilder.Entity<AccountCharacterData>()
                .ToTable("Accounts_Characters");
            modelBuilder.Entity<AccountCharacterData>()
                .HasKey(x => x.Id);
            modelBuilder.Entity<AccountCharacterData>()
                .HasOne<AccountData>(x => x.Account)
                    .WithMany(x => x.Characters)
                        .HasForeignKey(x => x.AccountId);
            modelBuilder.Entity<AccountCharacterData>()
                .Property(x => x.CharacterItemId).IsRequired();
        }
        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
