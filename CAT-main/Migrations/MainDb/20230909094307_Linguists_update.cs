using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAT.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class Linguists_update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LinguistLanguagePairs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceLanguage = table.Column<int>(type: "int", nullable: false),
                    TargetLanguage = table.Column<int>(type: "int", nullable: false),
                    Speciality = table.Column<int>(type: "int", nullable: false),
                    Roles = table.Column<int>(type: "int", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinguistLanguagePairs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Linguists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LinguistsLanguagePairsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Linguists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Linguists_LinguistLanguagePairs_LinguistsLanguagePairsId",
                        column: x => x.LinguistsLanguagePairsId,
                        principalTable: "LinguistLanguagePairs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Linguists_LinguistsLanguagePairsId",
                table: "Linguists",
                column: "LinguistsLanguagePairsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Linguists");

            migrationBuilder.DropTable(
                name: "LinguistLanguagePairs");
        }
    }
}
