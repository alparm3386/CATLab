using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAT.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class AnalysisAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Analysis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    SourceLanguage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TargetLanguage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Speciality = table.Column<int>(type: "int", nullable: false),
                    Repetitions = table.Column<int>(type: "int", nullable: false),
                    Match_100 = table.Column<int>(type: "int", nullable: false),
                    Match_101 = table.Column<int>(type: "int", nullable: false),
                    Match_50_74 = table.Column<int>(type: "int", nullable: false),
                    Match_75_84 = table.Column<int>(type: "int", nullable: false),
                    Match_85_94 = table.Column<int>(type: "int", nullable: false),
                    Match_95_99 = table.Column<int>(type: "int", nullable: false),
                    No_match = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Analysis", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Analysis_DocumentId",
                table: "Analysis",
                column: "DocumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Analysis");
        }
    }
}
