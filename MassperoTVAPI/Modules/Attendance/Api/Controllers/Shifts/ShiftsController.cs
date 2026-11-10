using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MassperoTVAPI.Shared.Application;using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Shifts;

namespace MassperoTVAPI.Modules.AttendanceModule.Api.Controllers.Shifts;

[ApiController]
[Route("api/attendance/shifts")]
[Authorize(Roles = "Admin,HR")]
public class ShiftsController : ControllerBase
{
    private readonly IShiftService _shiftService;

    public ShiftsController(IShiftService shiftService) => _shiftService = shiftService;

    /// <summary>List shifts. Optional filters: name (contains, case-insensitive) and isActive. Ordered by name.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ShiftDto>>>> GetAll([FromQuery] string? name, [FromQuery] bool? isActive, CancellationToken ct)
    {
        var result = await _shiftService.GetAllAsync(name, isActive, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Create a shift. StartTime accepts "HH:mm" or "HH:mm:ss".</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ShiftDto>>> Create([FromBody] CreateShiftDto dto, CancellationToken ct)
    {
        var result = await _shiftService.CreateAsync(dto, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Edit a shift (all fields, including IsActive).</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ShiftDto>>> Update(int id, [FromBody] UpdateShiftDto dto, CancellationToken ct)
    {
        var result = await _shiftService.UpdateAsync(id, dto, ct);
        return StatusCode(result.StatusCode, result);
    }
}