using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAT.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class ClientRateUpdate4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClientRates_RateId",
                table: "ClientRates");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRates_RateId",
                table: "ClientRates",
                column: "RateId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClientRates_RateId",
                table: "ClientRates");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRates_RateId",
                table: "ClientRates",
                column: "RateId");
        }
    }
}
