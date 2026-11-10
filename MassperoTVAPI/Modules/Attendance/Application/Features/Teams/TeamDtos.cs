namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Teams;

public record TeamDto(
    int Id,
    string Name,
    int EmployeeCount,
    List<TeamMemberDto> Employees
);

/// <summary>Only the employee Id and display name are exposed.</summary>
public record TeamMemberDto(
    int Id,
    string Name
);

public record CreateTeamDto
{
    public string? Name { get; init; }

    /// <summary>Optional. Employees to put in the team.</summary>
    public List<int>? EmployeeIds { get; init; }
}

/// <summary>EmployeeIds omitted/null = members unchanged; present (even empty) = membership REPLACED.</summary>
public record UpdateTeamDto : CreateTeamDto;

public record TeamMemberConflictDto(
    int EmployeeId,
    string CurrentTeamName
);
