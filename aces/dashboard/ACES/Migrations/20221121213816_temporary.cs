using Microsoft.EntityFrameworkCore.Migrations;

namespace ACES.Migrations
{
    public partial class temporary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JSONFiles",
                table: "Assignment");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JSONFiles",
                table: "Assignment",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
