using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ACES.Migrations
{
    public partial class AssignmentModelUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<string>(
                name: "CanvasLink",
                table: "Assignment",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Assignment",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "CanvasLink",
                table: "Assignment");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Assignment");

            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "Assignment");

            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "Enrollment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "Assignment",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
