using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Teams;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;

/// <summary>
/// Attendance-owned access to the HR Team table and Employee.TeamId (named to avoid clashing with any HR TeamRepository).
/// Never saves; never touches any other Employee field.
/// </summary>
public interface ITeamMembershipRepository : IGenericRepository<Team>
{
    Task<List<TeamDto>> SearchAsync(string? name, CancellationToken ct = default);
    Task<Team?> GetForUpdateAsync(int id, CancellationToken ct = default);
    Task<bool> TeamExistsAsync(int id, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default);
    Task<List<int>> GetMemberIdsAsync(int teamId, CancellationToken ct = default);

    /// <summary>TRACKED employees with the given ids, plus (when set) the current members of that team, in ONE query.</summary>
    Task<List<Employee>> GetEmployeesForUpdateAsync(IEnumerable<int> employeeIds, int? alsoMembersOfTeamId = null, CancellationToken ct = default);

    Task<Dictionary<int, string>> GetTeamNamesAsync(IEnumerable<int> teamIds, CancellationToken ct = default);
}
