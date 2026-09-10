using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MassperoTVAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviewScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InterviewMode",
                table: "Interviews",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InterviewDate",
                table: "Interviews",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "StartTime",
                table: "Interviews",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "EndTime",
                table: "Interviews",
                type: "time",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "InterviewMode", table: "Interviews");
            migrationBuilder.DropColumn(name: "InterviewDate", table: "Interviews");
            migrationBuilder.DropColumn(name: "StartTime", table: "Interviews");
            migrationBuilder.DropColumn(name: "EndTime", table: "Interviews");
        }
    }
}
