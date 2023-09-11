using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAT.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class LinguistsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Linguists_LinguistLanguagePairs_LinguistsLanguagePairsId",
                table: "Linguists");

            migrationBuilder.DropIndex(
                name: "IX_Linguists_LinguistsLanguagePairsId",
                table: "Linguists");

            migrationBuilder.DropColumn(
                name: "LinguistsLanguagePairsId",
                table: "Linguists");

            migrationBuilder.AddColumn<int>(
                name: "LinguistId",
                table: "LinguistLanguagePairs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LinguistLanguagePairs_LinguistId",
                table: "LinguistLanguagePairs",
                column: "LinguistId");

            migrationBuilder.AddForeignKey(
                name: "FK_LinguistLanguagePairs_Linguists_LinguistId",
                table: "LinguistLanguagePairs",
                column: "LinguistId",
                principalTable: "Linguists",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LinguistLanguagePairs_Linguists_LinguistId",
                table: "LinguistLanguagePairs");

            migrationBuilder.DropIndex(
                name: "IX_LinguistLanguagePairs_LinguistId",
                table: "LinguistLanguagePairs");

            migrationBuilder.DropColumn(
                name: "LinguistId",
                table: "LinguistLanguagePairs");

            migrationBuilder.AddColumn<int>(
                name: "LinguistsLanguagePairsId",
                table: "Linguists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Linguists_LinguistsLanguagePairsId",
                table: "Linguists",
                column: "LinguistsLanguagePairsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Linguists_LinguistLanguagePairs_LinguistsLanguagePairsId",
                table: "Linguists",
                column: "LinguistsLanguagePairsId",
                principalTable: "LinguistLanguagePairs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
