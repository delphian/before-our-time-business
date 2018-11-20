﻿// <auto-generated />
using System;
using BeforeOurTime.Business.Modules.Core.Dbs.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BeforeOurTime.Business.Migrations.EFAccountModule
{
    [DbContext(typeof(EFAccountModuleContext))]
    [Migration("20181119055037_linking-acount-and-characters")]
    partial class linkingacountandcharacters
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.3-rtm-32065")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("BeforeOurTime.Models.Modules.Account.Models.Data.AccountCharacterData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("AccountId");

                    b.Property<Guid>("CharacterItemId");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.ToTable("Accounts_Characters");
                });

            modelBuilder.Entity("BeforeOurTime.Models.Modules.Account.Models.Data.AccountData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Admin");

                    b.Property<Guid>("DataItemId");

                    b.Property<string>("DataType");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("Password");

                    b.Property<bool>("Temporary");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("BeforeOurTime.Models.Modules.Account.Models.Data.AccountCharacterData", b =>
                {
                    b.HasOne("BeforeOurTime.Models.Modules.Account.Models.Data.AccountData", "Account")
                        .WithMany("Characters")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
