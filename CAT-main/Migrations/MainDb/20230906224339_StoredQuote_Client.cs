using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAT.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class StoredQuote_Client : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StoredQuotes_ClientId",
                table: "StoredQuotes",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoredQuotes_Clients_ClientId",
                table: "StoredQuotes",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoredQuotes_Clients_ClientId",
                table: "StoredQuotes");

            migrationBuilder.DropIndex(
                name: "IX_StoredQuotes_ClientId",
                table: "StoredQuotes");
        }
    }
}
