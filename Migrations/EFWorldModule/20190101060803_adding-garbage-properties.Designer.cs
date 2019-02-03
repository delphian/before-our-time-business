﻿// <auto-generated />
using System;
using BeforeOurTime.Business.Modules.World.Dbs.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BeforeOurTime.Business.Migrations.EFWorldModule
{
    [DbContext(typeof(EFWorldModuleContext))]
    [Migration("20190101060803_adding-garbage-properties")]
    partial class addinggarbageproperties
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.3-rtm-32065")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("BeforeOurTime.Models.Modules.World.ItemProperties.Characters.CharacterItemData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("DataItemId");

                    b.Property<bool>("Temporary");

                    b.HasKey("Id");

                    b.ToTable("Item_Data_Characters");
                });

            modelBuilder.Entity("BeforeOurTime.Models.Modules.World.ItemProperties.Exits.ExitItemData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("DataItemId");

                    b.Property<Guid>("DestinationLocationId");

                    b.Property<int>("Direction");

                    b.Property<int>("Effort");

                    b.Property<int>("Time");

                    b.HasKey("Id");

                    b.ToTable("Item_Data_Exits");
                });

            modelBuilder.Entity("BeforeOurTime.Models.Modules.World.ItemProperties.Games.GameItemData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("DataItemId");

                    b.Property<Guid?>("DefaultLocationId");

                    b.HasKey("Id");

                    b.ToTable("Item_Data_Games");
                });

            modelBuilder.Entity("BeforeOurTime.Models.Modules.World.ItemProperties.Garbages.GarbageItemData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("DataItemId");

                    b.Property<int>("Interval");

                    b.Property<DateTime?>("IntervalTime");

                    b.HasKey("Id");

                    b.ToTable("Item_Data_Garbages");
                });

            modelBuilder.Entity("BeforeOurTime.Models.Modules.World.ItemProperties.Generators.GeneratorItemData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("DataItemId");

                    b.Property<int>("Interval");

                    b.Property<DateTime?>("IntervalTime");

                    b.Property<string>("Json");

                    b.Property<int>("Maximum");

                    b.HasKey("Id");

                    b.ToTable("Item_Data_Generators");
                });

            modelBuilder.Entity("BeforeOurTime.Models.Modules.World.ItemProperties.Locations.LocationItemData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("DataItemId");

                    b.HasKey("Id");

                    b.ToTable("Item_Data_Locations");
                });

            modelBuilder.Entity("BeforeOurTime.Models.Modules.World.ItemProperties.Physicals.PhysicalItemData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("DataItemId");

                    b.Property<bool>("Mobile");

                    b.Property<int>("Weight");

                    b.HasKey("Id");

                    b.ToTable("Item_Data_Physical");
                });
#pragma warning restore 612, 618
        }
    }
}