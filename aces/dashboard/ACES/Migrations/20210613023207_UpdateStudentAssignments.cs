using Microsoft.EntityFrameworkCore.Migrations;

namespace ACES.Migrations
{
    public partial class UpdateStudentAssignments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JSONCode",
                table: "StudentAssignment",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JSONCode",
                table: "StudentAssignment");
        }
    }
}
