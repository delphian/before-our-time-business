using Microsoft.EntityFrameworkCore.Migrations;

namespace BeforeOurTime.Business.Migrations.EFWorldModule
{
    public partial class removingnamedescfromcharacteritemdata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Item_Data_Characters");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Item_Data_Characters");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Item_Data_Characters",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Item_Data_Characters",
                nullable: false,
                defaultValue: "");
        }
    }
}
