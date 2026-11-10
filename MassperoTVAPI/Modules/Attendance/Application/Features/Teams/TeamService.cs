using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Shared.Application;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Teams;

/// <summary>
/// The only Attendance code that writes HR data: Team.TeamName and Employee.TeamId, nothing else.
/// Employees are loaded TRACKED and only TeamId is set, so EF updates only that column (no ExecuteUpdate).
/// ITeamMembershipRepository is injected directly (it shares the scoped DbContext, so the single save covers it).
/// </summary>
public class TeamService : ITeamService
{
    private const int    NameMaxLength           = 100; // HR Team.TeamName column length
    private const string ConcurrencyErrorMessage = "Team data changed, please retry.";

    private readonly ITeamMembershipRepository _teams;
    private readonly IConflictAwareSaveChanges _saveChanges;

    public TeamService(ITeamMembershipRepository teams, IConflictAwareSaveChanges saveChanges)
    {
        _teams       = teams;
        _saveChanges = saveChanges;
    }

    public async Task<ApiResponse<List<TeamDto>>> GetAllAsync(string? name, CancellationToken ct = default)
        => ApiResponse<List<TeamDto>>.SuccessResponse(await _teams.SearchAsync(name, ct));

    public async Task<ApiResponse<TeamDto>> CreateAsync(CreateTeamDto dto, CancellationToken ct = default)
    {
        var nameError = ValidateName(dto.Name, out var name);
        if (nameError is not null)
            return ApiResponse<TeamDto>.ErrorResponse(nameError);

        if (await _teams.NameExistsAsync(name, null, ct))
            return ApiResponse<TeamDto>.ErrorResponse($"A team named '{name}' already exists.", 409);

        var employeeIds = (dto.EmployeeIds ?? new List<int>()).Distinct().ToList();
        var employees   = await _teams.GetEmployeesForUpdateAsync(employeeIds, null, ct);

        var missing = employeeIds.Except(employees.Select(e => e.Id)).OrderBy(id => id).ToList();
        if (missing.Count > 0)
            return ApiResponse<TeamDto>.NotFoundResponse($"Employee(s) not found: {string.Join(", ", missing)}.");

        // Nobody is moved silently: anyone already in a team blocks the request.
        var conflict = await BuildConflictAsync(employees.Where(e => e.TeamId is not null).ToList(), ct);
        if (conflict is not null)
            return conflict;

        var team = new Team { TeamName = name };
        await _teams.AddAsync(team);
        foreach (var employee in employees)
            employee.Team = team; // sets only Employee.TeamId when saved

        try
        {
            await _saveChanges.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return ApiResponse<TeamDto>.ErrorResponse(ConcurrencyErrorMessage, 409);
        }

        return ApiResponse<TeamDto>.CreatedResponse(ToDto(team, employees), "Team created successfully.");
    }

    public async Task<ApiResponse<TeamDto>> UpdateAsync(int id, UpdateTeamDto dto, CancellationToken ct = default)
    {
        var team = await _teams.GetForUpdateAsync(id, ct);
        if (team is null)
            return ApiResponse<TeamDto>.NotFoundResponse($"Team {id} not found.");

        var nameError = ValidateName(dto.Name, out var name);
        if (nameError is not null)
            return ApiResponse<TeamDto>.ErrorResponse(nameError);

        if (await _teams.NameExistsAsync(name, id, ct))
            return ApiResponse<TeamDto>.ErrorResponse($"A team named '{name}' already exists.", 409);

        List<Employee> members;
        if (dto.EmployeeIds is null)
        {
            // Members stay unchanged (loaded only for the response)
            members = await _teams.GetEmployeesForUpdateAsync(new List<int>(), id, ct);
        }
        else
        {
            // Membership REPLACED: listed + current members in ONE query
            var listedIds = dto.EmployeeIds.Distinct().ToList();
            var loaded    = await _teams.GetEmployeesForUpdateAsync(listedIds, id, ct);
            var listed    = listedIds.ToHashSet();

            var missing = listedIds.Except(loaded.Select(e => e.Id)).OrderBy(x => x).ToList();
            if (missing.Count > 0)
                return ApiResponse<TeamDto>.NotFoundResponse($"Employee(s) not found: {string.Join(", ", missing)}.");

            var conflict = await BuildConflictAsync(
                loaded.Where(e => listed.Contains(e.Id) && e.TeamId is not null && e.TeamId != id).ToList(), ct);
            if (conflict is not null)
                return conflict;

            foreach (var employee in loaded)
            {
                if (listed.Contains(employee.Id))
                {
                    if (employee.TeamId != id)
                        employee.TeamId = id;
                }
                else if (employee.TeamId == id)
                {
                    employee.TeamId = null; // removed from the team
                }
            }

            members = loaded.Where(e => listed.Contains(e.Id)).ToList();
        }

        if (team.TeamName != name)
            team.TeamName = name;

        try
        {
            await _saveChanges.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return ApiResponse<TeamDto>.ErrorResponse(ConcurrencyErrorMessage, 409);
        }

        return ApiResponse<TeamDto>.SuccessResponse(ToDto(team, members), "Team updated successfully.");
    }

    private async Task<ApiResponse<TeamDto>?> BuildConflictAsync(List<Employee> inOtherTeam, CancellationToken ct)
    {
        if (inOtherTeam.Count == 0)
            return null;

        var teamNames = await _teams.GetTeamNamesAsync(inOtherTeam.Select(e => e.TeamId!.Value), ct);
        var conflicts = inOtherTeam
            .OrderBy(e => e.Id)
            .Select(e => new TeamMemberConflictDto(e.Id, teamNames.TryGetValue(e.TeamId!.Value, out var teamName) ? teamName : string.Empty))
            .ToList();

        return ApiConflictResponse<TeamDto, TeamMemberConflictDto>.Create(
            $"{conflicts.Count} employee(s) already belong to another team. Nobody was moved.", conflicts);
    }

    private static string? ValidateName(string? rawName, out string name)
    {
        name = rawName?.Trim() ?? string.Empty;
        if (name.Length == 0)
            return "Name is required.";
        if (name.Length > NameMaxLength)
            return $"Name must be at most {NameMaxLength} characters.";
        return null;
    }

    private static TeamDto ToDto(Team team, List<Employee> members)
    {
        var employees = members.OrderBy(e => e.Name).ThenBy(e => e.Id)
                               .Select(e => new TeamMemberDto(e.Id, e.Name))
                               .ToList();
        return new TeamDto(team.Id, team.TeamName, employees.Count, employees);
    }
}
