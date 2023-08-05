using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CATWeb.Migrations
{
    public partial class tuStatusChanged : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdClient = table.Column<int>(type: "int", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FilterName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceLang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetLang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Analysis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateProcessed = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TranslationUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idJob = table.Column<int>(type: "int", nullable: false),
                    tuid = table.Column<int>(type: "int", nullable: false),
                    source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    context = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    locks = table.Column<int>(type: "int", nullable: false),
                    target = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<long>(type: "bigint", nullable: false)
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
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "TranslationUnits");
        }
    }
}
