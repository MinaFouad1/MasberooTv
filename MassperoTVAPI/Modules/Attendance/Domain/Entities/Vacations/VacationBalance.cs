using MassperoTVAPI.Modules.AttendanceModule.Domain.Common;
using MassperoTVAPI.Core.Entities;

namespace MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Vacations;

public class VacationBalance : AuditableEntity
{
    public int          EmployeeId     { get; set; }
    public Employee     Employee       { get; set; } = null!;

    public int          VacationTypeId { get; set; }
    public VacationType VacationType   { get; set; } = null!;

    public int          Year           { get; set; }
    public decimal      Entitled       { get; set; }
    public decimal      Used           { get; set; }
    public decimal      CarriedOver    { get; set; }
    public byte[]       RowVersion     { get; set; } = [];
}
