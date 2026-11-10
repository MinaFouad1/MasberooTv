using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Teams;
using MassperoTVAPI.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Teams;

/// <summary>
/// Reads/writes HR Team and Employee.TeamId for the Attendance Teams area.
/// Team and Employee have no IsDeleted/IsActive/end-date columns, so no such filter exists to apply.
/// </summary>
public class TeamMembershipRepository : GenericRepository<Team>, ITeamMembershipRepository
{
    public TeamMembershipRepository(ApplicationDbContext context) : base(context) { }

    public async Task<List<TeamDto>> SearchAsync(string? name, CancellationToken ct = default)
    {
        var query = _dbSet.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name))
        {
            var lowered = name.Trim().ToLower();
            query = query.Where(t => t.TeamName.ToLower().Contains(lowered));
        }

        // One query: teams + members (Id and display name only).
        var teams = await query
            .OrderBy(t => t.TeamName).ThenBy(t => t.Id)
            .Select(t => new
            {
                t.Id,
                t.TeamName,
                Members = _context.Employees
                                  .Where(e => e.TeamId == t.Id)
                                  .OrderBy(e => e.Name).ThenBy(e => e.Id)
                                  .Select(e => new TeamMemberDto(e.Id, e.Name))
                                  .ToList()
            })
            .ToListAsync(ct);

        return teams.Select(t => new TeamDto(t.Id, t.TeamName, t.Members.Count, t.Members)).ToList();
    }

    public async Task<Team?> GetForUpdateAsync(int id, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<bool> TeamExistsAsync(int id, CancellationToken ct = default)
        => await _dbSet.AnyAsync(t => t.Id == id, ct);

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default)
    {
        var lowered = name.ToLower();
        return await _dbSet.AnyAsync(t => t.TeamName.ToLower() == lowered
                                       && (excludeId == null || t.Id != excludeId.Value), ct);
    }

    public async Task<List<int>> GetMemberIdsAsync(int teamId, CancellationToken ct = default)
        => await _context.Employees.AsNoTracking()
                                   .Where(e => e.TeamId == teamId)
                                   .Select(e => e.Id)
                                   .ToListAsync(ct);

    public async Task<List<Employee>> GetEmployeesForUpdateAsync(IEnumerable<int> employeeIds, int? alsoMembersOfTeamId = null, CancellationToken ct = default)
    {
        var idList = employeeIds.Distinct().ToList();
        if (alsoMembersOfTeamId is int teamId)
            return await _context.Employees.Where(e => idList.Contains(e.Id) || e.TeamId == teamId).ToListAsync(ct);

        if (idList.Count == 0)
            return new List<Employee>();
        return await _context.Employees.Where(e => idList.Contains(e.Id)).ToListAsync(ct);
    }

    public async Task<Dictionary<int, string>> GetTeamNamesAsync(IEnumerable<int> teamIds, CancellationToken ct = default)
    {
        var idList = teamIds.Distinct().ToList();
        return await _dbSet.AsNoTracking()
                           .Where(t => idList.Contains(t.Id))
                           .ToDictionaryAsync(t => t.Id, t => t.TeamName, ct);
    }
}
