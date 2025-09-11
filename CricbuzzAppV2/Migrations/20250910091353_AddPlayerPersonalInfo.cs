using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CricbuzzAppV2.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerPersonalInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerPersonalInfos",
                columns: table => new
                {
                    PlayerPersonalInfoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BirthPlace = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Height = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BattingStyle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BowlingStyle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerPersonalInfos", x => x.PlayerPersonalInfoId);
                    table.ForeignKey(
                        name: "FK_PlayerPersonalInfos_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerPersonalInfos_PlayerId",
                table: "PlayerPersonalInfos",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerPersonalInfos");
        }
    }
}
