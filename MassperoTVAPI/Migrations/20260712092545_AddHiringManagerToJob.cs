using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MassperoTVAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddHiringManagerToJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HiringManagerId",
                table: "Jobs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_HiringManagerId",
                table: "Jobs",
                column: "HiringManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_AspNetUsers_HiringManagerId",
                table: "Jobs",
                column: "HiringManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_AspNetUsers_HiringManagerId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_HiringManagerId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "HiringManagerId",
                table: "Jobs");
        }
    }
}
