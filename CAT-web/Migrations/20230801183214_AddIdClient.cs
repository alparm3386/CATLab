using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CATWeb.Migrations
{
    public partial class AddIdClient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdClient",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdClient",
                table: "Jobs");
        }
    }
}
