using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TDDBackendStats.Migrations
{
    /// <inheritdoc />
    public partial class AddGameStatFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EndingScore",
                table: "GameStats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShopsVisited",
                table: "GameStats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimePlayed",
                table: "GameStats",
                type: "time(6)",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndingScore",
                table: "GameStats");

            migrationBuilder.DropColumn(
                name: "ShopsVisited",
                table: "GameStats");

            migrationBuilder.DropColumn(
                name: "TimePlayed",
                table: "GameStats");
        }
    }
}
