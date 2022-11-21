using Microsoft.EntityFrameworkCore.Migrations;

namespace ACES.Migrations
{
    public partial class @new : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Json",
                table: "Assignment");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Json",
                table: "Assignment",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
