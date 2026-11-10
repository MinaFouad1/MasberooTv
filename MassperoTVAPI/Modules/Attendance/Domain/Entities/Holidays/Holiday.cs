using MassperoTVAPI.Shared.Domain;

namespace MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Holidays;

public class Holiday : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public DateOnly HolidayDate { get; set; }
    public bool IsPaid { get; set; }

    public string HolidayType { get; set; } = "National";  // or company
}
