using BeforeOurTime.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOutTime.Repository.Dbs.EF
{
    public class MSSQLContext : BaseContext
    {
        public MSSQLContext() : base() { }
        public MSSQLContext(DbContextOptions<BaseContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var path = System.IO.Directory.GetCurrentDirectory();
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile($"{path}/appsettings.json");
            var Configuration = configurationBuilder.Build();
            var defaultConnection = Configuration.GetSection("ConnectionStrings").GetValue<string>("DefaultConnection");
            optionsBuilder.UseSqlServer(defaultConnection);
        }
    }
}
