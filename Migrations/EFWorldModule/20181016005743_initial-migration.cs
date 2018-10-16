using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BeforeOurTime.Business.Migrations.EFWorldModule
{
    public partial class initialmigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Item_Data_Characters",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DataItemId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item_Data_Characters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Item_Data_Exits",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DataItemId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    DestinationLocationId = table.Column<Guid>(nullable: false),
                    Time = table.Column<int>(nullable: false),
                    Effort = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item_Data_Exits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Item_Data_Games",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DataItemId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    DefaultLocationId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item_Data_Games", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Item_Data_Locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DataItemId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item_Data_Locations", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Item_Data_Characters");

            migrationBuilder.DropTable(
                name: "Item_Data_Exits");

            migrationBuilder.DropTable(
                name: "Item_Data_Games");

            migrationBuilder.DropTable(
                name: "Item_Data_Locations");
        }
    }
}
