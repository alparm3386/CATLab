using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAT_web.Migrations
{
    public partial class AddTranslationUnit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TranslationUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idJob = table.Column<int>(type: "int", nullable: false),
                    tuid = table.Column<int>(type: "int", nullable: false),
                    sourceText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    context = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    locks = table.Column<int>(type: "int", nullable: false),
                    targetText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranslationUnits", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TranslationUnits_idJob",
                table: "TranslationUnits",
                column: "idJob");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TranslationUnits");
        }
    }
}
