using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TDDBackendStats.Migrations
{
    /// <inheritdoc />
    public partial class AddHeroPowerToGameStat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeroPower",
                table: "GameStats",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeroPower",
                table: "GameStats");
        }
    }
}
