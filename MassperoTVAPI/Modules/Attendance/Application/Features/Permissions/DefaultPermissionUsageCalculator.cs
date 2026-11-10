namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Permissions;

/// <summary>
/// running = 0; for each request: before = running; running += Minutes;
/// Exceeded = max(0, running - Allowed) - max(0, before - Allowed).
/// The exceeded values always add up to max(0, total - Allowed).
/// </summary>
public class DefaultPermissionUsageCalculator : IPermissionUsageCalculator
{
    public PermissionUsageResult Calculate(IReadOnlyList<int> orderedMinutes, int allowedMinutes)
    {
        var exceeded = new List<int>(orderedMinutes.Count);
        var running  = 0;

        foreach (var minutes in orderedMinutes)
        {
            var before = running;
            running += minutes;
            exceeded.Add(Math.Max(0, running - allowedMinutes) - Math.Max(0, before - allowedMinutes));
        }

        return new PermissionUsageResult(exceeded, running);
    }
}
