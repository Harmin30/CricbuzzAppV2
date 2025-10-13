using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CricbuzzAppV2.Migrations
{
    /// <inheritdoc />
    public partial class FixPlayerRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlayerPersonalInfos_PlayerId",
                table: "PlayerPersonalInfos");

            migrationBuilder.AddColumn<int>(
                name: "PlayerId1",
                table: "PlayerStats",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStats_PlayerId1",
                table: "PlayerStats",
                column: "PlayerId1");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerPersonalInfos_PlayerId",
                table: "PlayerPersonalInfos",
                column: "PlayerId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerStats_Players_PlayerId1",
                table: "PlayerStats",
                column: "PlayerId1",
                principalTable: "Players",
                principalColumn: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerStats_Players_PlayerId1",
                table: "PlayerStats");

            migrationBuilder.DropIndex(
                name: "IX_PlayerStats_PlayerId1",
                table: "PlayerStats");

            migrationBuilder.DropIndex(
                name: "IX_PlayerPersonalInfos_PlayerId",
                table: "PlayerPersonalInfos");

            migrationBuilder.DropColumn(
                name: "PlayerId1",
                table: "PlayerStats");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerPersonalInfos_PlayerId",
                table: "PlayerPersonalInfos",
                column: "PlayerId");
        }
    }
}
