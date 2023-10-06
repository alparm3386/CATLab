using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAT.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class DocumentFiltersUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DocumentFilters_DocumentId_FilterId",
                table: "DocumentFilters");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "DocumentFilters",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DocumentFilters",
                table: "DocumentFilters",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentFilters_DocumentId_FilterId",
                table: "DocumentFilters",
                columns: new[] { "DocumentId", "FilterId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DocumentFilters",
                table: "DocumentFilters");

            migrationBuilder.DropIndex(
                name: "IX_DocumentFilters_DocumentId_FilterId",
                table: "DocumentFilters");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "DocumentFilters");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentFilters_DocumentId_FilterId",
                table: "DocumentFilters",
                columns: new[] { "DocumentId", "FilterId" },
                unique: true);
        }
    }
}
