using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAT.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class filters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProfileId",
                table: "Filters",
                newName: "CompanyId");

            migrationBuilder.AddColumn<string>(
                name: "FileTypes",
                table: "Filters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileTypes",
                table: "Filters");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "Filters",
                newName: "ProfileId");
        }
    }
}
