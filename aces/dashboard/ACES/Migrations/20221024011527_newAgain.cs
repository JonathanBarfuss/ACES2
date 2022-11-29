using Microsoft.EntityFrameworkCore.Migrations;

namespace ACES.Migrations
{
    public partial class newAgain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JSON",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(nullable: true),
                    LineNumbers = table.Column<string>(nullable: true),
                    ReplaceWatermark = table.Column<bool>(nullable: false),
                    Comment = table.Column<string>(nullable: true),
                    WhitespaceLines = table.Column<string>(nullable: true),
                    RandomStringLines = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JSON", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JSON");
        }
    }
}
