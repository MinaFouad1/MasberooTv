using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MassperoTVAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddHoursPerferdJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "WorkingDaysPerWeek",
                table: "Candidates",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "WorkingHoursPerDay",
                table: "Candidates",
                type: "tinyint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkingDaysPerWeek",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "WorkingHoursPerDay",
                table: "Candidates");
        }
    }
}
