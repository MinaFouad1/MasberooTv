using DocumentFormat.OpenXml.Bibliography;
using MassperoTVAPI.Modules.Hr.Domain.Entities.Employees;
using MassperoTVAPI.Shared.Domain;
using Org.BouncyCastle.Pqc.Crypto.Lms;

namespace MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Vacations;

public class VacationBalance : AuditableEntity
{
    public int          EmployeeId     { get; set; }
    public Employee     Employee       { get; set; } = null!;

    public int          VacationTypeId { get; set; }
    public VacationType VacationType   { get; set; } = null!;

    public int          Year           { get; set; }
    public decimal      Entitled       { get; set; }  //Days the company gives her this year
    public decimal      Used           { get; set; }  //Days that has already taken
    public decimal      CarriedOver    { get; set; }  //Unused days that came from last year
    public byte[]       RowVersion     { get; set; } = [];
}
