using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAT.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class ClientRateUpdate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientRates_Clients_ClientId",
                table: "ClientRates");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "ClientRates",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_ClientRates_ClientId",
                table: "ClientRates",
                newName: "IX_ClientRates_CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientRates_Companies_CompanyId",
                table: "ClientRates",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientRates_Companies_CompanyId",
                table: "ClientRates");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "ClientRates",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_ClientRates_CompanyId",
                table: "ClientRates",
                newName: "IX_ClientRates_ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientRates_Clients_ClientId",
                table: "ClientRates",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
