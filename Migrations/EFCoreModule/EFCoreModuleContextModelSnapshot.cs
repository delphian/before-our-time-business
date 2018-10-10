﻿// <auto-generated />
using System;
using BeforeOurTime.Business.Modules.Core.Dbs.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BeforeOurTime.Business.Migrations.EFCoreModule
{
    [DbContext(typeof(EFCoreModuleContext))]
    partial class EFCoreModuleContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.3-rtm-32065")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("BeforeOurTime.Models.Modules.Core.Models.Data.CharacterData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("DataItemId");

                    b.Property<string>("Description");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Item_Data_Characters");
                });

            modelBuilder.Entity("BeforeOurTime.Models.Modules.Core.Models.Data.ExitData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("DataItemId");

                    b.Property<string>("Description");

                    b.Property<Guid>("DestinationLocationId");

                    b.Property<int>("Effort");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<int>("Time");

                    b.HasKey("Id");

                    b.ToTable("Item_Data_Exits");
                });

            modelBuilder.Entity("BeforeOurTime.Models.Modules.Core.Models.Data.GameData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("DataItemId");

                    b.Property<Guid?>("DefaultLocationId");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Item_Data_Games");
                });

            modelBuilder.Entity("BeforeOurTime.Models.Modules.Core.Models.Data.LocationData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("DataItemId");

                    b.Property<string>("Description");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Item_Data_Locations");
                });
#pragma warning restore 612, 618
        }
    }
}
