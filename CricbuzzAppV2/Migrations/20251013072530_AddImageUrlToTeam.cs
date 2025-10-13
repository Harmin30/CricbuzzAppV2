using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CricbuzzAppV2.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlToTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerStats_Players_PlayerId1",
                table: "PlayerStats");

            migrationBuilder.DropIndex(
                name: "IX_PlayerStats_PlayerId1",
                table: "PlayerStats");

            migrationBuilder.DropColumn(
                name: "PlayerId1",
                table: "PlayerStats");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Teams",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Teams");

            migrationBuilder.AddColumn<int>(
                name: "PlayerId1",
                table: "PlayerStats",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStats_PlayerId1",
                table: "PlayerStats",
                column: "PlayerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerStats_Players_PlayerId1",
                table: "PlayerStats",
                column: "PlayerId1",
                principalTable: "Players",
                principalColumn: "PlayerId");
        }
    }
}
