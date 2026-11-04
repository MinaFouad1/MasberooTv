using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Attendances;
using MassperoTVAPI.Shared.Domain;

namespace MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Permissions;

public class PermissionRequest : AuditableEntity
{
    public int            EmployeeId       { get; set; }
    public Employee       Employee         { get; set; } = null!;

    public int            PermissionTypeId { get; set; }
    public PermissionType PermissionType   { get; set; } = null!;

    public DateOnly       PermissionDate   { get; set; }
    public int            Minutes          { get; set; }
    public int            ExceededMinutes  { get; set; }
    public string         Reason           { get; set; } = string.Empty;
    public RequestStatus  Status           { get; set; }
    public int?           ApprovedBy       { get; set; }

    public int?           AttendanceId     { get; set; }
    public Attendance?    Attendance       { get; set; }
}
