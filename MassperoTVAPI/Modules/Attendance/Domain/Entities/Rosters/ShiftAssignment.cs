using MassperoTVAPI.Modules.AttendanceModule.Domain.Common;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Shifts;

namespace MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Rosters;

public class ShiftAssignment : AuditableEntity
{
    public int         RosterMonthId { get; set; }
    public RosterMonth RosterMonth   { get; set; } = null!;

    public int         EmployeeId    { get; set; }
    public Employee    Employee      { get; set; } = null!;

    public DateOnly    WorkDate      { get; set; }
    public DayType     DayType       { get; set; }

    public int?        ShiftId       { get; set; }
    public Shift?      Shift         { get; set; }

    public string?     Note          { get; set; }
    public byte[]      RowVersion    { get; set; } = [];
}
