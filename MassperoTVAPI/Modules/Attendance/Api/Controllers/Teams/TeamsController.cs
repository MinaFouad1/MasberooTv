using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MassperoTVAPI.Shared.Application;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Teams;

namespace MassperoTVAPI.Modules.AttendanceModule.Api.Controllers.Teams;

[ApiController]
[Route("api/attendance/teams")]
[Authorize(Roles = "Admin,HR")]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamsController(ITeamService teamService) => _teamService = teamService;

    /// <summary>List teams with their members, optionally filtered by name (contains).</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TeamDto>>>> GetAll([FromQuery] string? name, CancellationToken ct)
    {
        var result = await _teamService.GetAllAsync(name, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Create a team and optionally put employees in it.</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<TeamDto>>> Create([FromBody] CreateTeamDto dto, CancellationToken ct)
    {
        var result = await _teamService.CreateAsync(dto, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Rename a team; when employeeIds is given, replace its members.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<TeamDto>>> Update(int id, [FromBody] UpdateTeamDto dto, CancellationToken ct)
    {
        var result = await _teamService.UpdateAsync(id, dto, ct);
        return StatusCode(result.StatusCode, result);
    }
}
