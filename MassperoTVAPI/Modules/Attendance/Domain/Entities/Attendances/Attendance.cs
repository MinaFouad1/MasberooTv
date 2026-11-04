using MassperoTVAPI.Modules.AttendanceModule.Domain.Common;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Shifts;

namespace MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Attendances;

public class Attendance : AuditableEntity
{
    public int              EmployeeId        { get; set; }
    public Employee         Employee          { get; set; } = null!;

    public DateOnly         WorkDate          { get; set; }
    public DateTimeOffset?  CheckIn           { get; set; }
    public DateTimeOffset?  CheckOut          { get; set; }

    public int?             ShiftId           { get; set; }
    public Shift?           Shift             { get; set; }

    public DateTimeOffset?  ExpectedStart     { get; set; }
    public DateTimeOffset?  ExpectedEnd       { get; set; }

    public int              LateMinutes       { get; set; }
    public int              EarlyLeaveMinutes { get; set; }
    public int              WorkedMinutes     { get; set; }

    public AttendanceStatus Status            { get; set; }
    public bool             IsManualOverride  { get; set; }
    public string?          OverrideReason    { get; set; }
    public bool             IsLocked          { get; set; }
    public byte[]           RowVersion        { get; set; } = [];
}
