using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MassperoTVAPI.Migrations
{
    /// <inheritdoc />
    public partial class IsOpendDATEiJops : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeadLineDate",
                table: "Jobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOpend",
                table: "Jobs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeadLineDate",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "IsOpend",
                table: "Jobs");
        }
    }
}
