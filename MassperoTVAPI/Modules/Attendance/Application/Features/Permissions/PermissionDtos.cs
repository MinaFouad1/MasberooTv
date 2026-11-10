using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Permissions;

// -- Permission types -------------------------------------------------------

public record PermissionTypeDto(
    int Id,
    string Name,
    int DefaultMonthlyMinutes,
    bool IsActive
);

public record CreatePermissionTypeDto
{
    public string? Name { get; init; }

    /// <summary>Optional, default 0. Informational only: no logic uses it.</summary>
    public int? DefaultMonthlyMinutes { get; init; }

    /// <summary>Optional, default true.</summary>
    public bool? IsActive { get; init; }
}

public record UpdatePermissionTypeDto : CreatePermissionTypeDto;

// -- Permission balances ----------------------------------------------------

public record PermissionBalanceDto(
    int PermissionTypeId,
    string PermissionTypeName,
    int Year,
    int Month,
    int AllowedMinutes,
    int UsedMinutes,
    int RemainingMinutes,
    int ExceededMinutes
);

public record CreatePermissionBalanceItemDto
{
    public int PermissionTypeId { get; init; }

    /// <summary>Required (no default).</summary>
    public int? AllowedMinutes { get; init; }
}

public record CreatePermissionBalancesDto
{
    public int Year { get; init; }

    /// <summary>Optional. When omitted the balances are created for all 12 months.</summary>
    public List<int>? Months { get; init; }

    public List<CreatePermissionBalanceItemDto>? Items { get; init; }
}

public record UpdatePermissionBalanceDto
{
    public int PermissionTypeId { get; init; }
    public int Year { get; init; }
    public int Month { get; init; }

    /// <summary>Required.</summary>
    public int? AllowedMinutes { get; init; }
}

/// <summary>One (type, year, month) that already has a balance.</summary>
public record PermissionBalanceConflictDto(
    int PermissionTypeId,
    int Year,
    int Month
);

// -- Permission requests ----------------------------------------------------

public record PermissionRequestDto(
    int Id,
    int EmployeeId,
    int PermissionTypeId,
    string PermissionTypeName,
    DateOnly PermissionDate,
    int Minutes,
    int ExceededMinutes,
    bool IsExceeded,
    RequestStatus Status,
    string Reason,
    DateTimeOffset CreatedAt,
    int? MonthUsedMinutes,
    int? MonthAllowedMinutes
);

public record CreatePermissionRequestDto
{
    public int EmployeeId { get; init; }
    public int PermissionTypeId { get; init; }

    /// <summary>Date as "yyyy-MM-dd".</summary>
    public DateOnly? PermissionDate { get; init; }

    public int Minutes { get; init; }
    public string? Reason { get; init; }
}
