using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MassperoTVAPI.Shared.Application;using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Holidays;

namespace MassperoTVAPI.Modules.AttendanceModule.Api.Controllers.Holidays;

[ApiController]
[Route("api/attendance/holidays")]
[Authorize(Roles = "Admin,HR")]
public class HolidaysController : ControllerBase
{
    private readonly IHolidayService _holidayService;

    public HolidaysController(IHolidayService holidayService) => _holidayService = holidayService;

    /// <summary>List all holidays ordered by date.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<HolidayDto>>>> GetAll(CancellationToken ct)
    {
        var result = await _holidayService.GetAllAsync(ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Create a holiday. IsPaid defaults to true, HolidayType defaults to National.</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<HolidayDto>>> Create([FromBody] CreateHolidayDto dto, CancellationToken ct)
    {
        var result = await _holidayService.CreateAsync(dto, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Edit a holiday.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<HolidayDto>>> Update(int id, [FromBody] UpdateHolidayDto dto, CancellationToken ct)
    {
        var result = await _holidayService.UpdateAsync(id, dto, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Soft delete a holiday.</summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id, CancellationToken ct)
    {
        var result = await _holidayService.DeleteAsync(id, ct);
        return StatusCode(result.StatusCode, result);
    }
}