using Microsoft.EntityFrameworkCore.Migrations;

namespace ACES.Migrations
{
    public partial class updateCommit2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinesAdded",
                table: "Commit");

            migrationBuilder.DropColumn(
                name: "LinesDeleted",
                table: "Commit");

            migrationBuilder.DropColumn(
                name: "NumWatermarks",
                table: "Commit");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LinesAdded",
                table: "Commit",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LinesDeleted",
                table: "Commit",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumWatermarks",
                table: "Commit",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
