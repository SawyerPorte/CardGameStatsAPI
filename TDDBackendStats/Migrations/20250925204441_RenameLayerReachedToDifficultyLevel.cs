using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TDDBackendStats.Migrations
{
    /// <inheritdoc />
    public partial class RenameLayerReachedToDifficultyLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LayerReached",
                table: "GameStats",
                newName: "DifficultyLevel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DifficultyLevel",
                table: "GameStats",
                newName: "LayerReached");
        }
    }
}
