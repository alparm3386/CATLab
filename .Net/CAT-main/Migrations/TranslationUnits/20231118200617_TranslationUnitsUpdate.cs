using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAT.Migrations.TranslationUnits
{
    /// <inheritdoc />
    public partial class TranslationUnitsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "idJob",
                table: "TranslationUnits",
                newName: "documentId");

            migrationBuilder.RenameIndex(
                name: "IX_TranslationUnits_idJob",
                table: "TranslationUnits",
                newName: "IX_TranslationUnits_documentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "documentId",
                table: "TranslationUnits",
                newName: "idJob");

            migrationBuilder.RenameIndex(
                name: "IX_TranslationUnits_documentId",
                table: "TranslationUnits",
                newName: "IX_TranslationUnits_idJob");
        }
    }
}
