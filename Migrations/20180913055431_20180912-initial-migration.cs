using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BeforeOurTime.Business.Migrations
{
    public partial class _20180912initialmigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Item_Data_Games");
        }
    }
}
