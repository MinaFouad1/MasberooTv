namespace MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions;

/// <summary>Checks employees without depending on HR services or HR repositories.</summary>
public interface IEmployeeLookup
{
    Task<bool> ExistsAsync(int employeeId, CancellationToken ct = default);

    /// <summary>Returns which of the given employee ids exist, in ONE query.</summary>
    Task<List<int>> GetExistingIdsAsync(IEnumerable<int> employeeIds, CancellationToken ct = default);
}