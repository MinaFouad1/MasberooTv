using MassperoTVAPI.Modules.AttendanceModule.Domain.Common;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;
using MassperoTVAPI.Core.Entities;

namespace MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Vacations;

public class VacationRequest : AuditableEntity
{
    public int           EmployeeId     { get; set; }
    public Employee      Employee       { get; set; } = null!;

    public int           VacationTypeId { get; set; }
    public VacationType  VacationType   { get; set; } = null!;

    public DateOnly      FromDate       { get; set; }
    public DateOnly      ToDate         { get; set; }
    public decimal       Days           { get; set; }
    public RequestStatus Status         { get; set; }
    public int?          ApprovedBy     { get; set; }
    public string        Reason         { get; set; } = string.Empty;
    public byte[]        RowVersion     { get; set; } = [];
}
