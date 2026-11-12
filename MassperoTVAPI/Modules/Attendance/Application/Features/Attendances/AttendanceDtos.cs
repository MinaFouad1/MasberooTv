using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Attendances;

// -- Requests -----------------------------------------------------------------

/// <summary>Body of POST api/attendance/attendances/manual.</summary>
public record CreateManualAttendanceDto
{
    public int EmployeeId { get; init; }

    public DateOnly? WorkDate { get; init; }

    /// <summary>Optional. Omitted: the employee's single Work shift on WorkDate, or unscheduled when there is none.</summary>
    public int? ShiftId { get; init; }

    /// <summary>Required.</summary>
    public DateTimeOffset? CheckIn { get; init; }

    /// <summary>Optional.</summary>
    public DateTimeOffset? CheckOut { get; init; }

    /// <summary>Optional, max 500 characters.</summary>
    public string? Reason { get; init; }
}

/// <summary>Query of GET api/attendance/attendances. All filters optional, combined with AND.</summary>
public record AttendanceSearchQueryDto
{
    /// <summary>Inclusive, on WorkDate.</summary>
    public DateOnly? StartDate { get; init; }

    /// <summary>Inclusive, on WorkDate.</summary>
    public DateOnly? EndDate { get; init; }

    public int? ShiftId { get; init; }

    public int? EmployeeId { get; init; }

    /// <summary>Case-insensitive contains on the employee name.</summary>
    public string? EmployeeName { get; init; }

    public int? JobId { get; init; }

    /// <summary>Rows whose employee has an Approved permission of this type on the row's WorkDate.</summary>
    public int? PermissionTypeId { get; init; }

    /// <summary>AttendanceStatus name, case-insensitive.</summary>
    public string? Status { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

/// <summary>Validated filter passed to the repository.</summary>
public record AttendanceSearchFilter(
    DateOnly? StartDate,
    DateOnly? EndDate,
    int? ShiftId,
    int? EmployeeId,
    string? EmployeeName,
    int? JobId,
    int? PermissionTypeId,
    AttendanceStatus? Status
);

// -- Responses ----------------------------------------------------------------

/// <summary>One attendance row. Date-times are the stored UTC values; convert them with TimeZoneId.</summary>
public class AttendanceDto
{
    public int Id { get; init; }
    public int EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public int? JobId { get; init; }
    public string? JobName { get; init; }
    public DateOnly WorkDate { get; init; }
    public int? ShiftId { get; init; }
    public string? ShiftName { get; init; }
    public string? TimeZoneId { get; init; }
    public DateTimeOffset? ExpectedStart { get; init; }
    public DateTimeOffset? ExpectedEnd { get; init; }
    public DateTimeOffset? CheckIn { get; init; }
    public DateTimeOffset? CheckOut { get; init; }
    public int LateMinutes { get; init; }
    public int EarlyLeaveMinutes { get; init; }
    public int WorkedMinutes { get; init; }
    public AttendanceStatus Status { get; init; }
    public bool IsManualOverride { get; init; }
    public bool HasPermission => Permissions.Count > 0;
    public List<AttendancePermissionDto> Permissions { get; set; } = new();
}

/// <summary>An Approved permission of the employee on the row's WorkDate (displayed only).</summary>
public record AttendancePermissionDto(
    int PermissionRequestId,
    int PermissionTypeId,
    string PermissionTypeName,
    int Minutes,
    int ExceededMinutes
);

/// <summary>Repository row: an Approved permission with its employee and date, grouped in memory onto attendance rows.</summary>
public record AttendanceDayPermissionDto(
    int EmployeeId,
    DateOnly PermissionDate,
    int PermissionRequestId,
    int PermissionTypeId,
    string PermissionTypeName,
    int Minutes,
    int ExceededMinutes
);

/// <summary>Statistics over ALL rows matching the filters (not only the page).</summary>
public record AttendanceStatisticsDto(
    int TotalRecords,
    double AvgLateMinutes,
    int TotalBreakOverages,
    int TotalAbsences
);

/// <summary>Repository result of the single aggregate query. AvgLateMinutes is null when no row is late.</summary>
public record AttendanceStatisticsRow(
    int TotalRecords,
    double? AvgLateMinutes,
    int TotalAbsences
);

/// <summary>Paged search result (same pattern as the project's paged wrapper, plus statistics).</summary>
public record AttendancePagedResultDto(
    List<AttendanceDto> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages,
    AttendanceStatisticsDto Statistics
);
