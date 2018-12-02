using Microsoft.EntityFrameworkCore.Migrations;

namespace BeforeOurTime.Business.Migrations.EFWorldModule
{
    public partial class removingnamedescfromexititemdata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Item_Data_Exits");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Item_Data_Exits");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Item_Data_Exits",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Item_Data_Exits",
                nullable: false,
                defaultValue: "");
        }
    }
}
