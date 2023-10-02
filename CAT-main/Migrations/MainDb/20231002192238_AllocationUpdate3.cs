using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAT.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class AllocationUpdate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Allocations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Allocations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
