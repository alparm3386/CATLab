using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAT.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class RateCompositeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfigConstants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigConstants", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rates_SourceLanguageId_TargetLanguageId_Speciality_Task",
                table: "Rates",
                columns: new[] { "SourceLanguageId", "TargetLanguageId", "Speciality", "Task" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigConstants");

            migrationBuilder.DropIndex(
                name: "IX_Rates_SourceLanguageId_TargetLanguageId_Speciality_Task",
                table: "Rates");
        }
    }
}
