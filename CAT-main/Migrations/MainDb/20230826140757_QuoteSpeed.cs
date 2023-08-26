using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAT.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class QuoteSpeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Speed",
                table: "Quotes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Speed",
                table: "Quotes");
        }
    }
}
