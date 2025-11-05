using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CricbuzzAppV2.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchResultDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResultDescription",
                table: "Matches",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResultDescription",
                table: "Matches");
        }
    }
}
