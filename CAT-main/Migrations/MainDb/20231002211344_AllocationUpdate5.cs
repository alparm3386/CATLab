using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAT.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class AllocationUpdate5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CompletionDate",
                table: "Allocations",
                newName: "DeallocationDate");

            migrationBuilder.AddColumn<string>(
                name: "AllocatedBy",
                table: "Allocations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeallocatedBy",
                table: "Allocations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllocatedBy",
                table: "Allocations");

            migrationBuilder.DropColumn(
                name: "DeallocatedBy",
                table: "Allocations");

            migrationBuilder.RenameColumn(
                name: "DeallocationDate",
                table: "Allocations",
                newName: "CompletionDate");
        }
    }
}
