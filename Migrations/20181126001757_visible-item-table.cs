using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BeforeOurTime.Business.Migrations
{
    public partial class visibleitemtable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TerminalId",
                table: "Items");

            migrationBuilder.CreateTable(
                name: "Item_Data_Visible",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DataItemId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item_Data_Visible", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Item_Data_Visible");

            migrationBuilder.AddColumn<Guid>(
                name: "TerminalId",
                table: "Items",
                nullable: true);
        }
    }
}
