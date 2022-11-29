using Microsoft.EntityFrameworkCore.Migrations;

namespace ACES.Migrations
{
    public partial class ChangeReplace : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "ReplaceWatermark",
                table: "Assignment",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ReplaceWatermark",
                table: "Assignment",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool));
        }
    }
}
