using MassperoTVAPI.Shared.Domain;

namespace MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Permissions;

public class PermissionType : AuditableEntity
{
    public string Name                  { get; set; } = string.Empty;
    public int    DefaultMonthlyMinutes { get; set; }
    public bool   IsActive              { get; set; }
}
