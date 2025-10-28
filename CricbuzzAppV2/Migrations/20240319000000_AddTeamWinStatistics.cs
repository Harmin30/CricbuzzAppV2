using Microsoft.EntityFrameworkCore.Migrations;

namespace CricbuzzAppV2.Migrations
{
    public partial class AddTeamWinStatistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
    migrationBuilder.AddColumn<int>(
              name: "ODIWins",
   table: "Teams",
    type: "int",
      nullable: true);

      migrationBuilder.AddColumn<int>(
   name: "T20Wins",
                table: "Teams",
     type: "int",
      nullable: true);

        migrationBuilder.AddColumn<int>(
    name: "TestWins",
      table: "Teams",
  type: "int",
      nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
  migrationBuilder.DropColumn(
     name: "ODIWins",
      table: "Teams");

    migrationBuilder.DropColumn(
                name: "T20Wins",
            table: "Teams");

            migrationBuilder.DropColumn(
      name: "TestWins",
              table: "Teams");
}
    }
}