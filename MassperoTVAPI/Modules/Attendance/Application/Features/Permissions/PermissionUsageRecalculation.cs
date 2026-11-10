using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Permissions;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Permissions;

/// <summary>Shared RECOMPUTE step for one (employee, type, year, month) group. In memory only; the caller saves.</summary>
internal static class PermissionUsageRecalculation
{
    /// <summary>orderedApprovedRequests: approved, non-deleted requests of the group, ordered by PermissionDate then Id.</summary>
    public static void Apply(IPermissionUsageCalculator calculator, PermissionBalance balance,
        IReadOnlyList<PermissionRequest> orderedApprovedRequests, DateTimeOffset now)
    {
        var result = calculator.Calculate(orderedApprovedRequests.Select(r => r.Minutes).ToList(), balance.AllowedMinutes);

        for (var i = 0; i < orderedApprovedRequests.Count; i++)
        {
            var request  = orderedApprovedRequests[i];
            var exceeded = result.ExceededMinutes[i];
            if (request.ExceededMinutes == exceeded)
                continue; // update only the requests that changed

            request.ExceededMinutes = exceeded;
            if (request.Id != 0)
                request.UpdatedAt = now;
        }

        balance.UsedMinutes = result.TotalUsedMinutes;

        // Always touch the balance so its UPDATE (and its RowVersion check) runs even when the totals did not change.
        balance.UpdatedAt = now;
    }
}
