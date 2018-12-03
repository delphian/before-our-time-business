using Microsoft.EntityFrameworkCore.Migrations;

namespace BeforeOurTime.Business.Migrations.EFWorldModule
{
    public partial class removingnamedescfromlocationproperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Item_Data_Locations");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Item_Data_Locations",
                newName: "DataType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DataType",
                table: "Item_Data_Locations",
                newName: "Description");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Item_Data_Locations",
                nullable: false,
                defaultValue: "");
        }
    }
}
