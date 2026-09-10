using MassperoTVAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MassperoTVAPI.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260909110000_AddInterviewEvaluator")]
public partial class AddInterviewEvaluator : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "EvaluatorId",
            table: "Interviews",
            type: "nvarchar(450)",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Interviews_EvaluatorId",
            table: "Interviews",
            column: "EvaluatorId");

        migrationBuilder.AddForeignKey(
            name: "FK_Interviews_AspNetUsers_EvaluatorId",
            table: "Interviews",
            column: "EvaluatorId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Interviews_AspNetUsers_EvaluatorId",
            table: "Interviews");

        migrationBuilder.DropIndex(
            name: "IX_Interviews_EvaluatorId",
            table: "Interviews");

        migrationBuilder.DropColumn(
            name: "EvaluatorId",
            table: "Interviews");
    }
}
