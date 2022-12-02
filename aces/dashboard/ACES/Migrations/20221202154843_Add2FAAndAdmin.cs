using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ACES.Migrations
{
    public partial class Add2FAAndAdmin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                table: "Student",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Instructor",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Instructor",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                table: "Instructor",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                table: "Student");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Instructor");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Instructor");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                table: "Instructor");
        }
    }
}
