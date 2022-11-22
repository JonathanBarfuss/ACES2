using Microsoft.EntityFrameworkCore.Migrations;

namespace ACES.Migrations
{
    public partial class TookOutNotMapped : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JSONFiles",
                table: "Assignment",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JSONFiles",
                table: "Assignment");
        }
    }
}
