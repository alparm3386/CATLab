using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAT.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class ClientRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceLanguageId",
                table: "ClientRates");

            migrationBuilder.DropColumn(
                name: "Speciality",
                table: "ClientRates");

            migrationBuilder.RenameColumn(
                name: "Task",
                table: "ClientRates",
                newName: "RateId");

            migrationBuilder.RenameColumn(
                name: "TargetLanguageId",
                table: "ClientRates",
                newName: "Currency");

            migrationBuilder.RenameColumn(
                name: "Rate",
                table: "ClientRates",
                newName: "RateToClient");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRates_RateId",
                table: "ClientRates",
                column: "RateId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientRates_Rates_RateId",
                table: "ClientRates",
                column: "RateId",
                principalTable: "Rates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientRates_Rates_RateId",
                table: "ClientRates");

            migrationBuilder.DropIndex(
                name: "IX_ClientRates_RateId",
                table: "ClientRates");

            migrationBuilder.RenameColumn(
                name: "RateToClient",
                table: "ClientRates",
                newName: "Rate");

            migrationBuilder.RenameColumn(
                name: "RateId",
                table: "ClientRates",
                newName: "Task");

            migrationBuilder.RenameColumn(
                name: "Currency",
                table: "ClientRates",
                newName: "TargetLanguageId");

            migrationBuilder.AddColumn<int>(
                name: "SourceLanguageId",
                table: "ClientRates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Speciality",
                table: "ClientRates",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
