using Microsoft.EntityFrameworkCore.Migrations;

namespace ACES.Migrations
{
    public partial class ChangeAssignments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Assignment",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Assignment",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LineNumbers",
                table: "Assignment",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RandomStringLines",
                table: "Assignment",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReplaceWatermark",
                table: "Assignment",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "WhitespaceLines",
                table: "Assignment",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Assignment");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Assignment");

            migrationBuilder.DropColumn(
                name: "LineNumbers",
                table: "Assignment");

            migrationBuilder.DropColumn(
                name: "RandomStringLines",
                table: "Assignment");

            migrationBuilder.DropColumn(
                name: "ReplaceWatermark",
                table: "Assignment");

            migrationBuilder.DropColumn(
                name: "WhitespaceLines",
                table: "Assignment");
        }
    }
}
