using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MassperoTVAPI.Migrations
{
    /// <inheritdoc />
    public partial class SeedStatusesAndClearances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Candidate Statuses (upsert — safe with existing FK references) ──
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT [Statuses] ON;
                MERGE INTO [Statuses] AS target
                USING (VALUES
                    (1, N'Offered'),
                    (2, N'Under Vetting'),
                    (3, N'Contracted'),
                    (4, N'On Board'),
                    (5, N'Ready for Training'),
                    (6, N'In Production')
                ) AS source ([Id], [Name])
                ON target.[Id] = source.[Id]
                WHEN MATCHED THEN
                    UPDATE SET target.[Name] = source.[Name]
                WHEN NOT MATCHED BY TARGET THEN
                    INSERT ([Id], [Name]) VALUES (source.[Id], source.[Name]);
                SET IDENTITY_INSERT [Statuses] OFF;
            ");

            // ── Security Clearance Statuses (upsert) ────────────────────────────
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT [SecurityClearanceStatuses] ON;
                MERGE INTO [SecurityClearanceStatuses] AS target
                USING (VALUES
                    (1, N'In Check'),
                    (2, N'Accepted'),
                    (3, N'Rejected')
                ) AS source ([Id], [Name])
                ON target.[Id] = source.[Id]
                WHEN MATCHED THEN
                    UPDATE SET target.[Name] = source.[Name]
                WHEN NOT MATCHED BY TARGET THEN
                    INSERT ([Id], [Name]) VALUES (source.[Id], source.[Name]);
                SET IDENTITY_INSERT [SecurityClearanceStatuses] OFF;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Only delete rows that are NOT referenced by any Candidate
            migrationBuilder.Sql(@"
                DELETE FROM [Statuses]
                WHERE [Id] NOT IN (SELECT DISTINCT [StatusId] FROM [Candidates]);

                DELETE FROM [SecurityClearanceStatuses]
                WHERE [Id] NOT IN (SELECT DISTINCT [SecurityClearanceId] FROM [Candidates]);
            ");
        }
    }
}
