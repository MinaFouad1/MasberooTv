using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MassperoTVAPI.Shared.Application;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Rosters;

namespace MassperoTVAPI.Modules.AttendanceModule.Api.Controllers.Rosters;

[ApiController]
[Route("api/attendance/rosters")]
[Authorize(Roles = "Admin,HR")]
public class RostersController : ControllerBase
{
    private readonly IRosterService _rosterService;

    public RostersController(IRosterService rosterService) => _rosterService = rosterService;

    /// <summary>Bulk assign shifts / days off to a team and/or employees for one month.</summary>
    [HttpPost("assignments")]
    public async Task<ActionResult<ApiResponse<RosterAssignmentResultDto>>> Assign([FromBody] BulkRosterAssignmentDto dto, CancellationToken ct)
    {
        var result = await _rosterService.AssignAsync(dto, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Replace the whole day of one employee. Date as "yyyy-MM-dd".</summary>
    [HttpPut("employees/{employeeId:int}/days/{date}")]
    public async Task<ActionResult<ApiResponse<List<RosterRowDto>>>> EditDay(int employeeId, DateOnly date, [FromBody] EditRosterDayDto dto, CancellationToken ct)
    {
        var result = await _rosterService.EditDayAsync(employeeId, date, dto, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>View the schedule of a month, grouped by employee and day.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RosterEmployeeScheduleDto>>>> GetSchedule(
        [FromQuery] int year, [FromQuery] int month, [FromQuery] int? teamId, [FromQuery] int? employeeId, CancellationToken ct)
    {
        var result = await _rosterService.GetScheduleAsync(year, month, teamId, employeeId, ct);
        return StatusCode(result.StatusCode, result);
    }
}
