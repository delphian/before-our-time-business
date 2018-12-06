using Microsoft.EntityFrameworkCore.Migrations;

namespace BeforeOurTime.Business.Migrations
{
    public partial class removingitemtypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Items");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Items",
                nullable: false,
                defaultValue: 0);
        }
    }
}
