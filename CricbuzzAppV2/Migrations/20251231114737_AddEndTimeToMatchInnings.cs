using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CricbuzzAppV2.Migrations
{
    /// <inheritdoc />
    public partial class AddEndTimeToMatchInnings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "MatchInnings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "MatchInnings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "MatchInnings");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "MatchInnings");
        }
    }
}
