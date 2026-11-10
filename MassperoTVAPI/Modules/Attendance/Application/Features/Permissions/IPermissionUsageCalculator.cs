namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Permissions;

/// <summary>
/// Replaceable rule: given the approved requests of one (employee, type, month) in order,
/// returns how many minutes of each request exceed the monthly allowance, and the total used.
/// </summary>
public interface IPermissionUsageCalculator
{
    PermissionUsageResult Calculate(IReadOnlyList<int> orderedMinutes, int allowedMinutes);
}

/// <param name="ExceededMinutes">One value per input request, same order.</param>
/// <param name="TotalUsedMinutes">Sum of all minutes.</param>
public record PermissionUsageResult(IReadOnlyList<int> ExceededMinutes, int TotalUsedMinutes);
