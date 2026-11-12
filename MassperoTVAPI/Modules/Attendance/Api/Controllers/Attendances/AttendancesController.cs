using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MassperoTVAPI.Shared.Application;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Attendances;

namespace MassperoTVAPI.Modules.AttendanceModule.Api.Controllers.Attendances;

[ApiController]
[Route("api/attendance/attendances")]
[Authorize(Roles = "Admin,HR")]
public class AttendancesController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendancesController(IAttendanceService attendanceService) => _attendanceService = attendanceService;

    /// <summary>
    /// Search attendance. All filters optional (AND). Ordered by WorkDate desc, employee name, CheckIn, Id.
    /// Statistics describe ALL matching rows, not only the page. totalBreakOverages is a placeholder (always 0).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<AttendancePagedResultDto>>> Search([FromQuery] AttendanceSearchQueryDto query, CancellationToken ct)
    {
        var result = await _attendanceService.SearchAsync(query, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Create a manual attendance record (IsManualOverride = true) with its Manual punches.</summary>
    [HttpPost("manual")]
    public async Task<ActionResult<ApiResponse<AttendanceDto>>> CreateManual([FromBody] CreateManualAttendanceDto dto, CancellationToken ct)
    {
        var result = await _attendanceService.CreateManualAsync(dto, ct);
        return StatusCode(result.StatusCode, result);
    }
}
