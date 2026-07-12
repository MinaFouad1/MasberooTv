using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MassperoTVAPI.Migrations
{
    /// <inheritdoc />
    public partial class iSSUES : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Resolved",
                table: "Issues");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Issues",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Issues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Issues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Issues_CreatedByUserId",
                table: "Issues",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_AspNetUsers_CreatedByUserId",
                table: "Issues",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issues_AspNetUsers_CreatedByUserId",
                table: "Issues");

            migrationBuilder.DropIndex(
                name: "IX_Issues_CreatedByUserId",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Issues");

            migrationBuilder.AddColumn<bool>(
                name: "Resolved",
                table: "Issues",
                type: "bit",
                nullable: true);
        }
    }
}
