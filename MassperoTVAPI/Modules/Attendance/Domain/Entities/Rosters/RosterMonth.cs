using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;
using MassperoTVAPI.Shared.Domain;
using MassperoTVAPI.Modules.Hr.Domain.Entities.Teams;

namespace MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Rosters;

public class RosterMonth : AuditableEntity
{
    public int?         TeamId      { get; set; }
    public Team?        Team        { get; set; }

    public int          Year        { get; set; }
    public int          Month       { get; set; }
    public RosterStatus Status      { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public int?         PublishedBy { get; set; }
}
