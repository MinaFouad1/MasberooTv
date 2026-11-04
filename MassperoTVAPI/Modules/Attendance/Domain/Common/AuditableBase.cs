namespace MassperoTVAPI.Modules.AttendanceModule.Domain.Common;

/// <summary>Audit columns shared by every Attendance table.</summary>
public abstract class AuditableBase
{
    public DateTimeOffset  CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public int?            CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public int?            UpdatedBy { get; set; }
    public bool            IsDeleted { get; set; }
}
