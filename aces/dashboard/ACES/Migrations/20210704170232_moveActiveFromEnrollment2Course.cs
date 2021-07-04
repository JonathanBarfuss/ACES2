using Microsoft.EntityFrameworkCore.Migrations;

namespace ACES.Migrations
{
    public partial class moveActiveFromEnrollment2Course : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Active",
                table: "Enrollment");

            migrationBuilder.AddColumn<bool>(
                name: "IsCourseActive",
                table: "Course",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCourseActive",
                table: "Course");

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "Enrollment",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
