using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Models;
using Microsoft.Extensions.Configuration;
using BeforeOurTime.Models.Modules.Account.Models.Data;
using BeforeOutTime.Business.Dbs.EF;

namespace BeforeOurTime.Business.Modules.Terminal.Dbs.EF
{
    /// <summary>
    /// Entity framework database context
    /// </summary>
    public class EFTerminalModuleContext : BaseContext
    {
        public DbSet<AccountData> Accounts { set; get; }
        public DbSet<AccountCharacterData> AccountCharacters { set; get; }
        public EFTerminalModuleContext() : base() { }
        public EFTerminalModuleContext(DbContextOptions<BaseContext> options) : base(options) { }
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
        }
        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
