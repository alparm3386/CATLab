using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAT_web.Migrations
{
    public partial class addDateProcessed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateProcessed",
                table: "Jobs",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateProcessed",
                table: "Jobs");
        }
    }
}
