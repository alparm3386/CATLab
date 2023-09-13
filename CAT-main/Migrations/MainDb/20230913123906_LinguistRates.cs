using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAT.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class LinguistRates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceLanguageId",
                table: "LinguistRates");

            migrationBuilder.DropColumn(
                name: "Speciality",
                table: "LinguistRates");

            migrationBuilder.RenameColumn(
                name: "Task",
                table: "LinguistRates",
                newName: "RateId");

            migrationBuilder.RenameColumn(
                name: "TargetLanguageId",
                table: "LinguistRates",
                newName: "Currency");

            migrationBuilder.RenameColumn(
                name: "Rate",
                table: "LinguistRates",
                newName: "RateToLinguist");

            migrationBuilder.CreateIndex(
                name: "IX_LinguistRates_LinguistId",
                table: "LinguistRates",
                column: "LinguistId");

            migrationBuilder.CreateIndex(
                name: "IX_LinguistRates_RateId",
                table: "LinguistRates",
                column: "RateId");

            migrationBuilder.AddForeignKey(
                name: "FK_LinguistRates_Linguists_LinguistId",
                table: "LinguistRates",
                column: "LinguistId",
                principalTable: "Linguists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LinguistRates_Rates_RateId",
                table: "LinguistRates",
                column: "RateId",
                principalTable: "Rates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LinguistRates_Linguists_LinguistId",
                table: "LinguistRates");

            migrationBuilder.DropForeignKey(
                name: "FK_LinguistRates_Rates_RateId",
                table: "LinguistRates");

            migrationBuilder.DropIndex(
                name: "IX_LinguistRates_LinguistId",
                table: "LinguistRates");

            migrationBuilder.DropIndex(
                name: "IX_LinguistRates_RateId",
                table: "LinguistRates");

            migrationBuilder.RenameColumn(
                name: "RateToLinguist",
                table: "LinguistRates",
                newName: "Rate");

            migrationBuilder.RenameColumn(
                name: "RateId",
                table: "LinguistRates",
                newName: "Task");

            migrationBuilder.RenameColumn(
                name: "Currency",
                table: "LinguistRates",
                newName: "TargetLanguageId");

            migrationBuilder.AddColumn<int>(
                name: "SourceLanguageId",
                table: "LinguistRates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Speciality",
                table: "LinguistRates",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
