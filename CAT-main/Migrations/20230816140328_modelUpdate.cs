using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAT.Migrations
{
    /// <inheritdoc />
    public partial class modelUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateProcessed",
                table: "Jobs",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateProcessed",
                table: "Jobs");
        }
    }
}
