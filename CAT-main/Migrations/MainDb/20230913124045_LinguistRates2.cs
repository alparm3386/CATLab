﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAT.Migrations.MainDb
{
    /// <inheritdoc />
    public partial class LinguistRates2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LinguistRates_RateId",
                table: "LinguistRates");

            migrationBuilder.CreateIndex(
                name: "IX_LinguistRates_RateId",
                table: "LinguistRates",
                column: "RateId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LinguistRates_RateId",
                table: "LinguistRates");

            migrationBuilder.CreateIndex(
                name: "IX_LinguistRates_RateId",
                table: "LinguistRates",
                column: "RateId");
        }
    }
}
