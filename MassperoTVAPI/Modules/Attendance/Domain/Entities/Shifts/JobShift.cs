using MassperoTVAPI.Shared.Domain;
using MassperoTVAPI.Modules.Hr.Domain.Entities.Jobs;

namespace MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Shifts;

public class JobShift : AuditableBase
{
    public int   JobId   { get; set; }
    public Job   Job     { get; set; } = null!;

    public int   ShiftId { get; set; }
    public Shift Shift   { get; set; } = null!;
}
