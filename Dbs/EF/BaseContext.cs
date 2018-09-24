using BeforeOurTime.Models;
using BeforeOurTime.Models.Accounts;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.ItemAttributes.Players;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Models.ItemAttributes.Characters;
using BeforeOurTime.Models.ItemAttributes;
using BeforeOurTime.Models.ItemAttributes.Exits;
using BeforeOurTime.Models.ItemAttributes.Physicals;
using BeforeOurTime.Models.Primitives.Images;
using BeforeOurTime.Models.ItemAttributes.Visibles;

namespace BeforeOutTime.Repository.Dbs.EF
{
    /// <summary>
    /// Entity framework database context
    /// </summary>
    public class BaseContext : DbContext
    {
        public DbSet<Account> Accounts { set; get;  }
        // Items
        public DbSet<Item> Items { set; get; }
        public DbSet<VisibleAttribute> Visibles { set; get; }
        public DbSet<PlayerAttribute> Players { set; get; }
        public DbSet<PhysicalAttribute> Physicals { set; get; }
        public DbSet<ExitAttribute> Exits { set; get; }

        public BaseContext() : base() { }
        public BaseContext(DbContextOptions<BaseContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<ItemAttribute>();
            // Account
            modelBuilder.Entity<Account>()
                .ToTable("Accounts")
                .HasIndex(account => account.Name).IsUnique(true);
            modelBuilder.Entity<Account>()
                .HasKey(account => account.Id);
            modelBuilder.Entity<Account>()
                .Ignore(account => account.Characters)
                .Property(x => x.Name).IsRequired();
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
            // Item Attribute Visible
            modelBuilder.Entity<VisibleAttribute>()
                .ToTable("Item_Attribute_Visibles");
            modelBuilder.Entity<VisibleAttribute>()
                .HasKey(x => x.Id);
            modelBuilder.Entity<VisibleAttribute>()
                .HasOne(x => x.Item)
                .WithOne()
                .HasForeignKey<VisibleAttribute>(x => x.ItemId);
            modelBuilder.Entity<VisibleAttribute>()
                .Ignore(x => x.AttributeType);
            // Item Attribute Player
            modelBuilder.Entity<PlayerAttribute>()
                .ToTable("Item_Attribute_Players");
            modelBuilder.Entity<PlayerAttribute>()
                .HasKey(x => x.Id);
            modelBuilder.Entity<PlayerAttribute>()
                .HasOne(x => x.Item)
                .WithOne()
                .HasForeignKey<PlayerAttribute>(x => x.ItemId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PlayerAttribute>()
                .Ignore(x => x.AttributeType)
                .Property(x => x.AccountId).IsRequired();
            modelBuilder.Entity<PlayerAttribute>()
                .HasOne<Account>()
                .WithMany()
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
            // Item Attribute Physical
            modelBuilder.Entity<PhysicalAttribute>()
                .ToTable("Item_Attribute_Physicals");
            modelBuilder.Entity<PhysicalAttribute>()
                .HasKey(x => x.Id);
            modelBuilder.Entity<PhysicalAttribute>()
                .HasOne(x => x.Item)
                .WithOne()
                .HasForeignKey<PhysicalAttribute>(x => x.ItemId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PhysicalAttribute>()
                .Ignore(x => x.AttributeType);
            // Item Attribute Character
            modelBuilder.Entity<CharacterAttribute>()
                .ToTable("Item_Attribute_Characters");
            modelBuilder.Entity<CharacterAttribute>()
                .HasKey(x => x.Id);
            modelBuilder.Entity<CharacterAttribute>()
                .HasOne(x => x.Item)
                .WithOne()
                .HasForeignKey<CharacterAttribute>(x => x.ItemId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ExitAttribute>()
                .Ignore(x => x.AttributeType);
            // Item Character Health
            modelBuilder.Entity<CharacterHealth>()
                .ToTable("Item_Attribute_Characters_Health");
            // Item Attribute Exit
            modelBuilder.Entity<ExitAttribute>()
                .ToTable("Item_Attribute_Exits");
            modelBuilder.Entity<ExitAttribute>()
                .HasKey(x => x.Id);
            modelBuilder.Entity<ExitAttribute>()
                .HasOne(x => x.Item)
                .WithOne()
                .HasForeignKey<ExitAttribute>(x => x.ItemId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ExitAttribute>()
                .Ignore(x => x.AttributeType);
            modelBuilder.Entity<ExitAttribute>()
                .HasOne(x => x.DestinationLocation)
                .WithMany()
                .HasForeignKey(x => x.DestinationLocationId)
                .OnDelete(DeleteBehavior.Restrict);
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
