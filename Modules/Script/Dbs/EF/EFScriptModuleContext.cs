using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Models;
using Microsoft.Extensions.Configuration;
using BeforeOurTime.Models.Modules.Account.Models.Data;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOutTime.Business.Dbs.EF;
using BeforeOurTime.Models.Modules.Script.ItemProperties.Javascripts;

namespace BeforeOurTime.Business.Modules.Script.Dbs.EF
{
    /// <summary>
    /// Entity framework database context
    /// </summary>
    public class EFScriptModuleContext : BaseContext
    {
        public DbSet<JavascriptItemData> Javascripts { set; get; }
        public EFScriptModuleContext() : base() { }
        public EFScriptModuleContext(DbContextOptions<BaseContext> options) : base(options) { }
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
            // Item Javascript Data
            modelBuilder.Entity<JavascriptItemData>()
                .ToTable("Item_Data_Javascripts");
            modelBuilder.Entity<JavascriptItemData>()
                .HasKey(x => x.Id);
            modelBuilder.Entity<JavascriptItemData>()
                .Ignore(x => x.DataType);
        }
    }
}
