using Microsoft.EntityFrameworkCore.Migrations;

namespace ACES.Migrations
{
    public partial class splitJsonFromAssignment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Assignment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Assignment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LineNumbers",
                table: "Assignment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RandomStringLines",
                table: "Assignment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReplaceWatermark",
                table: "Assignment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WhitespaceLines",
                table: "Assignment",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
