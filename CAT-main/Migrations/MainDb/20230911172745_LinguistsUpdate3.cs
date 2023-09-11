using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAT.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class LinguistsUpdate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AddressId",
                table: "Linguists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Linguists_AddressId",
                table: "Linguists",
                column: "AddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Linguists_Addresses_AddressId",
                table: "Linguists",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Linguists_Addresses_AddressId",
                table: "Linguists");

            migrationBuilder.DropIndex(
                name: "IX_Linguists_AddressId",
                table: "Linguists");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "Linguists");
        }
    }
}
