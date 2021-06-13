using Microsoft.EntityFrameworkCore.Migrations;

namespace ACES.Migrations
{
    public partial class UpdateCommit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JSONCode",
                table: "Commit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JSONCode",
                table: "Commit");
        }
    }
}
