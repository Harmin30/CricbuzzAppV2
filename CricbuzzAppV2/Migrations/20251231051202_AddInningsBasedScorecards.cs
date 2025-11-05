using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CricbuzzAppV2.Migrations
{
    /// <inheritdoc />
    public partial class AddInningsBasedScorecards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ElectedToBat",
                table: "Matches",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxInningsPerTeam",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OversLimit",
                table: "Matches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TossWinnerTeamId",
                table: "Matches",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MatchInnings",
                columns: table => new
                {
                    MatchInningsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchId = table.Column<int>(type: "int", nullable: false),
                    BattingTeamId = table.Column<int>(type: "int", nullable: false),
                    BowlingTeamId = table.Column<int>(type: "int", nullable: false),
                    InningsNumber = table.Column<int>(type: "int", nullable: false),
                    TotalRuns = table.Column<int>(type: "int", nullable: false),
                    WicketsLost = table.Column<int>(type: "int", nullable: false),
                    OversBowled = table.Column<double>(type: "float", nullable: false),
                    Extras = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchInnings", x => x.MatchInningsId);
                    table.ForeignKey(
                        name: "FK_MatchInnings_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "MatchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchInnings_Teams_BattingTeamId",
                        column: x => x.BattingTeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchInnings_Teams_BowlingTeamId",
                        column: x => x.BowlingTeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BattingScorecards",
                columns: table => new
                {
                    BattingScorecardId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchInningsId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Runs = table.Column<int>(type: "int", nullable: false),
                    BallsFaced = table.Column<int>(type: "int", nullable: false),
                    Fours = table.Column<int>(type: "int", nullable: false),
                    Sixes = table.Column<int>(type: "int", nullable: false),
                    HowOut = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BattingPosition = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BattingScorecards", x => x.BattingScorecardId);
                    table.ForeignKey(
                        name: "FK_BattingScorecards_MatchInnings_MatchInningsId",
                        column: x => x.MatchInningsId,
                        principalTable: "MatchInnings",
                        principalColumn: "MatchInningsId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BattingScorecards_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BowlingScorecards",
                columns: table => new
                {
                    BowlingScorecardId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchInningsId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Overs = table.Column<double>(type: "float", nullable: false),
                    Maidens = table.Column<int>(type: "int", nullable: false),
                    RunsConceded = table.Column<int>(type: "int", nullable: false),
                    Wickets = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BowlingScorecards", x => x.BowlingScorecardId);
                    table.ForeignKey(
                        name: "FK_BowlingScorecards_MatchInnings_MatchInningsId",
                        column: x => x.MatchInningsId,
                        principalTable: "MatchInnings",
                        principalColumn: "MatchInningsId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BowlingScorecards_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BattingScorecards_MatchInningsId",
                table: "BattingScorecards",
                column: "MatchInningsId");

            migrationBuilder.CreateIndex(
                name: "IX_BattingScorecards_PlayerId",
                table: "BattingScorecards",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_BowlingScorecards_MatchInningsId",
                table: "BowlingScorecards",
                column: "MatchInningsId");

            migrationBuilder.CreateIndex(
                name: "IX_BowlingScorecards_PlayerId",
                table: "BowlingScorecards",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchInnings_BattingTeamId",
                table: "MatchInnings",
                column: "BattingTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchInnings_BowlingTeamId",
                table: "MatchInnings",
                column: "BowlingTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchInnings_MatchId",
                table: "MatchInnings",
                column: "MatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BattingScorecards");

            migrationBuilder.DropTable(
                name: "BowlingScorecards");

            migrationBuilder.DropTable(
                name: "MatchInnings");

            migrationBuilder.DropColumn(
                name: "ElectedToBat",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "MaxInningsPerTeam",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "OversLimit",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TossWinnerTeamId",
                table: "Matches");
        }
    }
}
