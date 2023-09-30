using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAT.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class LinguistLanguagesUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LinguistLanguagePairs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LinguistLanguagePairs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LinguistId = table.Column<int>(type: "int", nullable: true),
                    Roles = table.Column<int>(type: "int", nullable: false),
                    SourceLanguage = table.Column<int>(type: "int", nullable: false),
                    Speciality = table.Column<int>(type: "int", nullable: false),
                    TargetLanguage = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinguistLanguagePairs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LinguistLanguagePairs_Linguists_LinguistId",
                        column: x => x.LinguistId,
                        principalTable: "Linguists",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LinguistLanguagePairs_LinguistId",
                table: "LinguistLanguagePairs",
                column: "LinguistId");
        }
    }
}
