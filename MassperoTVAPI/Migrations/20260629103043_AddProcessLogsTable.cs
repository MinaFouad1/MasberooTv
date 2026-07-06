using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MassperoTVAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessLogsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateId = table.Column<int>(type: "int", nullable: false),
                    OldProcessStatusId = table.Column<int>(type: "int", nullable: false),
                    NewProcessStatusId = table.Column<int>(type: "int", nullable: false),
                    ChangedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessLogs_AspNetUsers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProcessLogs_Candidates_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "Candidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessLogs_ProcessStatuses_NewProcessStatusId",
                        column: x => x.NewProcessStatusId,
                        principalTable: "ProcessStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessLogs_ProcessStatuses_OldProcessStatusId",
                        column: x => x.OldProcessStatusId,
                        principalTable: "ProcessStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessLogs_CandidateId",
                table: "ProcessLogs",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessLogs_ChangedByUserId",
                table: "ProcessLogs",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessLogs_NewProcessStatusId",
                table: "ProcessLogs",
                column: "NewProcessStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessLogs_OldProcessStatusId",
                table: "ProcessLogs",
                column: "OldProcessStatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessLogs");
        }
    }
}
