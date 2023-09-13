using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAT.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class ClientRateUpdate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ClientRates_ClientId",
                table: "ClientRates",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientRates_Clients_ClientId",
                table: "ClientRates",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientRates_Clients_ClientId",
                table: "ClientRates");

            migrationBuilder.DropIndex(
                name: "IX_ClientRates_ClientId",
                table: "ClientRates");
        }
    }
}
