using Microsoft.EntityFrameworkCore.Migrations;

namespace BeforeOurTime.Business.Migrations.EFScriptModule
{
    public partial class javascriptfunctionsitempropertyagain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Functions",
                table: "Item_Data_Javascripts",
                newName: "ScriptFunctions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ScriptFunctions",
                table: "Item_Data_Javascripts",
                newName: "Functions");
        }
    }
}
