using MassperoTVAPI.Shared.Domain;

namespace MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Vacations;

public class VacationType : AuditableEntity
{
    public string Name         { get; set; } = string.Empty;
    public bool   IsPaid       { get; set; }
    public int    DefaultDays  { get; set; }
    public bool   CanCarryOver { get; set; }
}
