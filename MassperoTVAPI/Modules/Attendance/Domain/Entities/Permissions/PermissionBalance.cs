using MassperoTVAPI.Modules.AttendanceModule.Domain.Common;
using MassperoTVAPI.Core.Entities;

namespace MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Permissions;

public class PermissionBalance : AuditableEntity
{
    public int            EmployeeId       { get; set; }
    public Employee       Employee         { get; set; } = null!;

    public int            PermissionTypeId { get; set; }
    public PermissionType PermissionType   { get; set; } = null!;

    public int            Year             { get; set; }
    public int            Month            { get; set; }
    public int            AllowedMinutes   { get; set; }
    public int            UsedMinutes      { get; set; }
    public byte[]         RowVersion       { get; set; } = [];
}
