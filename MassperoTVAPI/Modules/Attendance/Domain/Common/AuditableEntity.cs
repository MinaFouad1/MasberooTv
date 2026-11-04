namespace MassperoTVAPI.Modules.AttendanceModule.Domain.Common;

/// <summary>Base for Attendance entities that have a single int key.</summary>
public abstract class AuditableEntity : AuditableBase
{
    public int Id { get; set; }
}
