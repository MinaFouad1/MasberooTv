using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MassperoTVAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddHiringDateToCandidate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "HiringDate",
                table: "Candidates",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HiringDate",
                table: "Candidates");
        }
    }
}
