using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAT.Migrations.TranslationUnitsDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TranslationUnits");
        }
    }
}
