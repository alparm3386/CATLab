using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CAT.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class SpecialityFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "_Id_",
                table: "Specialities",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Specialities",
                table: "Specialities",
                column: "_Id_");

            migrationBuilder.InsertData(
                table: "Specialities",
                columns: new[] { "_Id_", "Id", "Name" },
                values: new object[,]
                {
                    { 1, 1, "General" },
                    { 2, 2, "Marketing" },
                    { 3, 3, "Technical" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Specialities",
                table: "Specialities");

            migrationBuilder.DeleteData(
                table: "Specialities",
                keyColumn: "_Id_",
                keyColumnType: "int",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Specialities",
                keyColumn: "_Id_",
                keyColumnType: "int",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Specialities",
                keyColumn: "_Id_",
                keyColumnType: "int",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "_Id_",
                table: "Specialities");
        }
    }
}
