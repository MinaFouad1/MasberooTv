using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Attendances;
using MassperoTVAPI.Shared.Domain;

namespace MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Vacations;

public class VacationTransaction : AuditableEntity
{
    public int                VacationBalanceId { get; set; }
    public VacationBalance    VacationBalance   { get; set; } = null!;

    public decimal            Days              { get; set; }
    public VacationTxReason   Reason            { get; set; }

    public int?               VacationRequestId { get; set; }
    public VacationRequest?   VacationRequest   { get; set; }

    public int?               AttendanceId      { get; set; }
    public Attendance?        Attendance        { get; set; }
}
