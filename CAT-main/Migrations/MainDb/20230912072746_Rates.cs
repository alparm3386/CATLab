using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAT.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class Rates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    SourceLanguageId = table.Column<int>(type: "int", nullable: false),
                    TargetLanguageId = table.Column<int>(type: "int", nullable: false),
                    Speciality = table.Column<int>(type: "int", nullable: false),
                    Task = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LinguistRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LinguistId = table.Column<int>(type: "int", nullable: false),
                    SourceLanguageId = table.Column<int>(type: "int", nullable: false),
                    TargetLanguageId = table.Column<int>(type: "int", nullable: false),
                    Speciality = table.Column<int>(type: "int", nullable: false),
                    Task = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinguistRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceLanguageId = table.Column<int>(type: "int", nullable: false),
                    TargetLanguageId = table.Column<int>(type: "int", nullable: false),
                    Speciality = table.Column<int>(type: "int", nullable: false),
                    Task = table.Column<int>(type: "int", nullable: false),
                    RateToClient = table.Column<float>(type: "real", nullable: false),
                    RateToTranslator = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rates", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientRates");

            migrationBuilder.DropTable(
                name: "LinguistRates");

            migrationBuilder.DropTable(
                name: "Rates");
        }
    }
}
