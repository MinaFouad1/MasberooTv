using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MassperoTVAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessStatusToCandidate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProcessStatusId",
                table: "Candidates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE [Candidates] SET [ProcessStatusId] = 1 WHERE [ProcessStatusId] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_ProcessStatusId",
                table: "Candidates",
                column: "ProcessStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Candidates_ProcessStatuses_ProcessStatusId",
                table: "Candidates",
                column: "ProcessStatusId",
                principalTable: "ProcessStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Candidates_ProcessStatuses_ProcessStatusId",
                table: "Candidates");

            migrationBuilder.DropIndex(
                name: "IX_Candidates_ProcessStatusId",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "ProcessStatusId",
                table: "Candidates");
        }
    }
}
