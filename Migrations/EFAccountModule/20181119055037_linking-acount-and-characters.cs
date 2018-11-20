using Microsoft.EntityFrameworkCore.Migrations;

namespace BeforeOurTime.Business.Migrations.EFAccountModule
{
    public partial class linkingacountandcharacters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Characters_AccountId",
                table: "Accounts_Characters",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Characters_Accounts_AccountId",
                table: "Accounts_Characters",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Characters_Accounts_AccountId",
                table: "Accounts_Characters");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_Characters_AccountId",
                table: "Accounts_Characters");
        }
    }
}
