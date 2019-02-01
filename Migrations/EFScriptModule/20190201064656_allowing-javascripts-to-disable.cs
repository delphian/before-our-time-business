using Microsoft.EntityFrameworkCore.Migrations;

namespace BeforeOurTime.Business.Migrations.EFScriptModule
{
    public partial class allowingjavascriptstodisable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Disabled",
                table: "Item_Data_Javascripts",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ErrCount",
                table: "Item_Data_Javascripts",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ErrDescriptions",
                table: "Item_Data_Javascripts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Disabled",
                table: "Item_Data_Javascripts");

            migrationBuilder.DropColumn(
                name: "ErrCount",
                table: "Item_Data_Javascripts");

            migrationBuilder.DropColumn(
                name: "ErrDescriptions",
                table: "Item_Data_Javascripts");
        }
    }
}
