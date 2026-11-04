BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    IF SCHEMA_ID(N'attendance') IS NULL EXEC(N'CREATE SCHEMA [attendance];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE TABLE [attendance].[Holidays] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(150) NOT NULL,
        [HolidayDate] date NOT NULL,
        [IsPaid] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] int NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedBy] int NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Holidays] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE TABLE [attendance].[PermissionTypes] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [DefaultMonthlyMinutes] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] int NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedBy] int NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_PermissionTypes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE TABLE [attendance].[Shifts] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [StartTime] time NOT NULL,
        [DurationMinutes] int NOT NULL,
        [BreakMinutes] int NOT NULL,
        [GraceMinutes] int NOT NULL,
        [TimeZoneId] nvarchar(100) NOT NULL DEFAULT N'Egypt Standard Time',
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] int NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedBy] int NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Shifts] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE TABLE [Teams] (
        [Id] int NOT NULL IDENTITY,
        [TeamName] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Teams] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE TABLE [attendance].[VacationTypes] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [IsPaid] bit NOT NULL,
        [DefaultDays] int NOT NULL,
        [CanCarryOver] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] int NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedBy] int NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_VacationTypes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE TABLE [attendance].[JobShifts] (
        [JobId] int NOT NULL,
        [ShiftId] int NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] int NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedBy] int NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_JobShifts] PRIMARY KEY ([JobId], [ShiftId]),
        CONSTRAINT [FK_JobShifts_Jobs_JobId] FOREIGN KEY ([JobId]) REFERENCES [Jobs] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_JobShifts_Shifts_ShiftId] FOREIGN KEY ([ShiftId]) REFERENCES [attendance].[Shifts] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE TABLE [Employees] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [EmploymentType] nvarchar(50) NULL,
        [JobId] int NULL,
        [TeamId] int NULL,
        [CandidateId] int NULL,
        CONSTRAINT [PK_Employees] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Employees_Candidates_CandidateId] FOREIGN KEY ([CandidateId]) REFERENCES [Candidates] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Employees_Jobs_JobId] FOREIGN KEY ([JobId]) REFERENCES [Jobs] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Employees_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [Teams] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE TABLE [attendance].[RosterMonths] (
        [Id] int NOT NULL IDENTITY,
        [TeamId] int NULL,
        [Year] int NOT NULL,
        [Month] int NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [PublishedAt] datetimeoffset NULL,
        [PublishedBy] int NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] int NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedBy] int NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_RosterMonths] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RosterMonths_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [Teams] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE TABLE [attendance].[AttendancePunches] (
        [Id] int NOT NULL IDENTITY,
        [EmployeeId] int NOT NULL,
        [PunchTime] datetimeoffset NOT NULL,
        [DeviceId] nvarchar(100) NULL,
        [Source] nvarchar(20) NOT NULL,
        [IsProcessed] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] int NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedBy] int NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_AttendancePunches] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AttendancePunches_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE TABLE [attendance].[Attendances] (
        [Id] int NOT NULL IDENTITY,
        [EmployeeId] int NOT NULL,
        [WorkDate] date NOT NULL,
        [CheckIn] datetimeoffset NULL,
        [CheckOut] datetimeoffset NULL,
        [ShiftId] int NULL,
        [ExpectedStart] datetimeoffset NULL,
        [ExpectedEnd] datetimeoffset NULL,
        [LateMinutes] int NOT NULL,
        [EarlyLeaveMinutes] int NOT NULL,
        [WorkedMinutes] int NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [IsManualOverride] bit NOT NULL,
        [OverrideReason] nvarchar(500) NULL,
        [IsLocked] bit NOT NULL,
        [RowVersion] rowversion NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] int NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedBy] int NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Attendances] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Attendances_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Attendances_Shifts_ShiftId] FOREIGN KEY ([ShiftId]) REFERENCES [attendance].[Shifts] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE TABLE [attendance].[PermissionBalances] (
        [Id] int NOT NULL IDENTITY,
        [EmployeeId] int NOT NULL,
        [PermissionTypeId] int NOT NULL,
        [Year] int NOT NULL,
        [Month] int NOT NULL,
        [AllowedMinutes] int NOT NULL,
        [UsedMinutes] int NOT NULL,
        [RowVersion] rowversion NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] int NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedBy] int NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_PermissionBalances] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PermissionBalances_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PermissionBalances_PermissionTypes_PermissionTypeId] FOREIGN KEY ([PermissionTypeId]) REFERENCES [attendance].[PermissionTypes] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE TABLE [attendance].[VacationBalances] (
        [Id] int NOT NULL IDENTITY,
        [EmployeeId] int NOT NULL,
        [VacationTypeId] int NOT NULL,
        [Year] int NOT NULL,
        [Entitled] decimal(5,2) NOT NULL,
        [Used] decimal(5,2) NOT NULL,
        [CarriedOver] decimal(5,2) NOT NULL,
        [RowVersion] rowversion NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] int NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedBy] int NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_VacationBalances] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VacationBalances_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_VacationBalances_VacationTypes_VacationTypeId] FOREIGN KEY ([VacationTypeId]) REFERENCES [attendance].[VacationTypes] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE TABLE [attendance].[VacationRequests] (
        [Id] int NOT NULL IDENTITY,
        [EmployeeId] int NOT NULL,
        [VacationTypeId] int NOT NULL,
        [FromDate] date NOT NULL,
        [ToDate] date NOT NULL,
        [Days] decimal(5,2) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [ApprovedBy] int NULL,
        [Reason] nvarchar(500) NOT NULL,
        [RowVersion] rowversion NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] int NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedBy] int NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_VacationRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VacationRequests_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_VacationRequests_VacationTypes_VacationTypeId] FOREIGN KEY ([VacationTypeId]) REFERENCES [attendance].[VacationTypes] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE TABLE [attendance].[ShiftAssignments] (
        [Id] int NOT NULL IDENTITY,
        [RosterMonthId] int NOT NULL,
        [EmployeeId] int NOT NULL,
        [WorkDate] date NOT NULL,
        [DayType] nvarchar(20) NOT NULL,
        [ShiftId] int NULL,
        [Note] nvarchar(500) NULL,
        [RowVersion] rowversion NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] int NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedBy] int NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_ShiftAssignments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ShiftAssignments_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ShiftAssignments_RosterMonths_RosterMonthId] FOREIGN KEY ([RosterMonthId]) REFERENCES [attendance].[RosterMonths] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ShiftAssignments_Shifts_ShiftId] FOREIGN KEY ([ShiftId]) REFERENCES [attendance].[Shifts] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE TABLE [attendance].[PermissionRequests] (
        [Id] int NOT NULL IDENTITY,
        [EmployeeId] int NOT NULL,
        [PermissionTypeId] int NOT NULL,
        [PermissionDate] date NOT NULL,
        [Minutes] int NOT NULL,
        [ExceededMinutes] int NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [ApprovedBy] int NULL,
        [AttendanceId] int NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] int NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedBy] int NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_PermissionRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PermissionRequests_Attendances_AttendanceId] FOREIGN KEY ([AttendanceId]) REFERENCES [attendance].[Attendances] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PermissionRequests_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PermissionRequests_PermissionTypes_PermissionTypeId] FOREIGN KEY ([PermissionTypeId]) REFERENCES [attendance].[PermissionTypes] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE TABLE [attendance].[VacationTransactions] (
        [Id] int NOT NULL IDENTITY,
        [VacationBalanceId] int NOT NULL,
        [Days] decimal(5,2) NOT NULL,
        [Reason] nvarchar(20) NOT NULL,
        [VacationRequestId] int NULL,
        [AttendanceId] int NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] int NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedBy] int NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_VacationTransactions] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_VacationTransactions_Request_Or_Attendance] CHECK (NOT ([VacationRequestId] IS NOT NULL AND [AttendanceId] IS NOT NULL)),
        CONSTRAINT [FK_VacationTransactions_Attendances_AttendanceId] FOREIGN KEY ([AttendanceId]) REFERENCES [attendance].[Attendances] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_VacationTransactions_VacationBalances_VacationBalanceId] FOREIGN KEY ([VacationBalanceId]) REFERENCES [attendance].[VacationBalances] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_VacationTransactions_VacationRequests_VacationRequestId] FOREIGN KEY ([VacationRequestId]) REFERENCES [attendance].[VacationRequests] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE INDEX [IX_AttendancePunches_EmployeeId_PunchTime] ON [attendance].[AttendancePunches] ([EmployeeId], [PunchTime]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE INDEX [IX_AttendancePunches_IsProcessed] ON [attendance].[AttendancePunches] ([IsProcessed]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Attendances_EmployeeId_WorkDate_ShiftId] ON [attendance].[Attendances] ([EmployeeId], [WorkDate], [ShiftId]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE INDEX [IX_Attendances_ShiftId] ON [attendance].[Attendances] ([ShiftId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Employees_CandidateId] ON [Employees] ([CandidateId]) WHERE [CandidateId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE INDEX [IX_Employees_JobId] ON [Employees] ([JobId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE INDEX [IX_Employees_TeamId] ON [Employees] ([TeamId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Holidays_HolidayDate] ON [attendance].[Holidays] ([HolidayDate]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE INDEX [IX_JobShifts_ShiftId] ON [attendance].[JobShifts] ([ShiftId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PermissionBalances_EmployeeId_PermissionTypeId_Year_Month] ON [attendance].[PermissionBalances] ([EmployeeId], [PermissionTypeId], [Year], [Month]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE INDEX [IX_PermissionBalances_PermissionTypeId] ON [attendance].[PermissionBalances] ([PermissionTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE INDEX [IX_PermissionRequests_AttendanceId] ON [attendance].[PermissionRequests] ([AttendanceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE INDEX [IX_PermissionRequests_EmployeeId] ON [attendance].[PermissionRequests] ([EmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE INDEX [IX_PermissionRequests_PermissionTypeId] ON [attendance].[PermissionRequests] ([PermissionTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_RosterMonths_TeamId_Year_Month] ON [attendance].[RosterMonths] ([TeamId], [Year], [Month]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ShiftAssignments_EmployeeId_WorkDate_ShiftId] ON [attendance].[ShiftAssignments] ([EmployeeId], [WorkDate], [ShiftId]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE INDEX [IX_ShiftAssignments_RosterMonthId] ON [attendance].[ShiftAssignments] ([RosterMonthId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE INDEX [IX_ShiftAssignments_ShiftId] ON [attendance].[ShiftAssignments] ([ShiftId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_VacationBalances_EmployeeId_VacationTypeId_Year] ON [attendance].[VacationBalances] ([EmployeeId], [VacationTypeId], [Year]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE INDEX [IX_VacationBalances_VacationTypeId] ON [attendance].[VacationBalances] ([VacationTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE INDEX [IX_VacationRequests_EmployeeId] ON [attendance].[VacationRequests] ([EmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE INDEX [IX_VacationRequests_VacationTypeId] ON [attendance].[VacationRequests] ([VacationTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE INDEX [IX_VacationTransactions_AttendanceId] ON [attendance].[VacationTransactions] ([AttendanceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE INDEX [IX_VacationTransactions_VacationBalanceId] ON [attendance].[VacationTransactions] ([VacationBalanceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    CREATE INDEX [IX_VacationTransactions_VacationRequestId] ON [attendance].[VacationTransactions] ([VacationRequestId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104124134_AddAttendanceModule'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261104124134_AddAttendanceModule', N'8.0.28');
END;
GO

COMMIT;
GO

