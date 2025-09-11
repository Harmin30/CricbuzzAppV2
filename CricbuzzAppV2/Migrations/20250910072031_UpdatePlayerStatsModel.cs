using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CricbuzzAppV2.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePlayerStatsModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BattingAverage",
                table: "PlayerStats");

            migrationBuilder.DropColumn(
                name: "BattingStyle",
                table: "PlayerStats");

            migrationBuilder.DropColumn(
                name: "BirthPlace",
                table: "PlayerStats");

            migrationBuilder.DropColumn(
                name: "Born",
                table: "PlayerStats");

            migrationBuilder.DropColumn(
                name: "BowlingAverage",
                table: "PlayerStats");

            migrationBuilder.DropColumn(
                name: "BowlingStyle",
                table: "PlayerStats");

            migrationBuilder.DropColumn(
                name: "DebutDate",
                table: "PlayerStats");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "PlayerStats");

            migrationBuilder.AddColumn<int>(
                name: "BallsFaced",
                table: "PlayerStats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RunsConceded",
                table: "PlayerStats",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BallsFaced",
                table: "PlayerStats");

            migrationBuilder.DropColumn(
                name: "RunsConceded",
                table: "PlayerStats");

            migrationBuilder.AddColumn<double>(
                name: "BattingAverage",
                table: "PlayerStats",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "BattingStyle",
                table: "PlayerStats",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BirthPlace",
                table: "PlayerStats",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "Born",
                table: "PlayerStats",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "BowlingAverage",
                table: "PlayerStats",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "BowlingStyle",
                table: "PlayerStats",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DebutDate",
                table: "PlayerStats",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Height",
                table: "PlayerStats",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
