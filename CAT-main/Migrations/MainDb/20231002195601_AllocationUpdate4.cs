using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAT.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class AllocationUpdate4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Allocations",
                type: "nvarchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Linguists_UserId",
                table: "Linguists",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Allocations_Linguists_UserId",
                table: "Allocations",
                column: "UserId",
                principalTable: "Linguists",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Allocations_Linguists_UserId",
                table: "Allocations");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Linguists_UserId",
                table: "Linguists");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Allocations",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)");
        }
    }
}
