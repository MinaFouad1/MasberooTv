using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MassperoTVAPI.Shared.Application;using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Vacations;

namespace MassperoTVAPI.Modules.AttendanceModule.Api.Controllers.Vacations;

[ApiController]
[Route("api/attendance/vacation-requests")]
[Authorize(Roles = "Admin,HR")]
public class VacationRequestsController : ControllerBase
{
    private readonly IVacationRequestService _vacationRequestService;

    public VacationRequestsController(IVacationRequestService vacationRequestService) => _vacationRequestService = vacationRequestService;

    /// <summary>List vacation requests (including cancelled), newest first. Optional filters: employeeId, status.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<VacationRequestDto>>>> GetAll([FromQuery] int? employeeId, [FromQuery] string? status, CancellationToken ct)
    {
        var result = await _vacationRequestService.GetAllAsync(employeeId, status, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Create a vacation request. It is approved automatically; days are calculated by the server.</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<VacationRequestDto>>> Create([FromBody] CreateVacationRequestDto dto, CancellationToken ct)
    {
        var result = await _vacationRequestService.CreateAsync(dto, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Withdraw (cancel) an approved vacation request and give the days back.</summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<VacationRequestDto>>> Withdraw(int id, CancellationToken ct)
    {
        var result = await _vacationRequestService.WithdrawAsync(id, ct);
        return StatusCode(result.StatusCode, result);
    }
}