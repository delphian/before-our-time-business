using Microsoft.EntityFrameworkCore.Migrations;

namespace BeforeOurTime.Business.Migrations.EFWorldModule
{
    public partial class removingdatatypefromlocationitemdata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataType",
                table: "Item_Data_Locations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataType",
                table: "Item_Data_Locations",
                nullable: true);
        }
    }
}
