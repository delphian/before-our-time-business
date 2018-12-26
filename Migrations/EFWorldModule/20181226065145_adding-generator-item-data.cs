using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BeforeOurTime.Business.Migrations.EFWorldModule
{
    public partial class addinggeneratoritemdata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Item_Data_Generators",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DataItemId = table.Column<Guid>(nullable: false),
                    Interval = table.Column<int>(nullable: false),
                    Maximum = table.Column<int>(nullable: false),
                    Json = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item_Data_Generators", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Item_Data_Generators");
        }
    }
}
