using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Permissions;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Permissions;

public static class PermissionMapper
{
    public static PermissionTypeDto ToDto(this PermissionType t) => new(
        t.Id,
        t.Name,
        t.DefaultMonthlyMinutes,
        t.IsActive
    );

    public static PermissionBalanceDto ToDto(this PermissionBalance b, string permissionTypeName) => new(
        b.PermissionTypeId,
        permissionTypeName,
        b.Year,
        b.Month,
        b.AllowedMinutes,
        b.UsedMinutes,
        Math.Max(0, b.AllowedMinutes - b.UsedMinutes),
        Math.Max(0, b.UsedMinutes - b.AllowedMinutes)
    );

    public static PermissionRequestDto ToDto(this PermissionRequest r, string permissionTypeName, PermissionBalance? monthBalance) => new(
        r.Id,
        r.EmployeeId,
        r.PermissionTypeId,
        permissionTypeName,
        r.PermissionDate,
        r.Minutes,
        r.ExceededMinutes,
        r.ExceededMinutes > 0,
        r.Status,
        r.Reason,
        r.CreatedAt,
        monthBalance?.UsedMinutes,
        monthBalance?.AllowedMinutes
    );
}
