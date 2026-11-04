using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Shared.Domain;

namespace MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Attendances;

public class AttendancePunch : AuditableEntity
{
    public int            EmployeeId  { get; set; }
    public Employee       Employee    { get; set; } = null!;

    public DateTimeOffset PunchTime   { get; set; }
    public string?        DeviceId    { get; set; }
    public PunchSource    Source      { get; set; }
    public bool           IsProcessed { get; set; }
}
