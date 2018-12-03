﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Models;
using Microsoft.Extensions.Configuration;
using BeforeOurTime.Models.Modules.Account.Models.Data;
using BeforeOurTime.Models.Modules.World.Models.Data;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOutTime.Business.Dbs.EF;
using BeforeOurTime.Models.Modules.World.ItemProperties.Locations;
using BeforeOurTime.Models.Modules.World.ItemProperties.Exits;

namespace BeforeOurTime.Business.Modules.World.Dbs.EF
{
    /// <summary>
    /// Entity framework database context
    /// </summary>
    public class EFWorldModuleContext : BaseContext
    {
        public DbSet<GameData> Games { set; get; }
        public DbSet<LocationItemData> Locations { set; get; }
        public DbSet<CharacterData> Characters { set; get; }
        public DbSet<ExitItemData> Exits { set; get; }
        public DbSet<PhysicalItemData> Physical { set; get; }
        public EFWorldModuleContext() : base() { }
        public EFWorldModuleContext(DbContextOptions<BaseContext> options) : base(options) { }
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
            modelBuilder.Entity<LocationItemData>()
                .ToTable("Item_Data_Locations");
            modelBuilder.Entity<LocationItemData>()
                .HasKey(x => x.Id);
            modelBuilder.Entity<LocationItemData>()
                .Ignore(x => x.DataType);
            // Item Location Data
            modelBuilder.Entity<CharacterData>()
                .ToTable("Item_Data_Characters");
            modelBuilder.Entity<CharacterData>()
                .HasKey(x => x.Id);
            modelBuilder.Entity<CharacterData>()
                .Ignore(x => x.DataType)
                .Property(x => x.Name).IsRequired();
            // Item Exit Data
            modelBuilder.Entity<ExitItemData>()
                .ToTable("Item_Data_Exits");
            modelBuilder.Entity<ExitItemData>()
                .HasKey(x => x.Id);
            modelBuilder.Entity<ExitItemData>()
                .Ignore(x => x.DataType);
            modelBuilder.Entity<ExitItemData>()
                .Property(x => x.DestinationLocationId).IsRequired();
            // Item Physical Data
            modelBuilder.Entity<PhysicalItemData>()
                .ToTable("Item_Data_Physical");
            modelBuilder.Entity<PhysicalItemData>()
                .HasKey(x => x.Id);
            modelBuilder.Entity<PhysicalItemData>()
                .Ignore(x => x.DataType);
        }
    }
}
