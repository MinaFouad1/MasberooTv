using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MassperoTVAPI.Shared.Application;using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Vacations;

namespace MassperoTVAPI.Modules.AttendanceModule.Api.Controllers.Vacations;

[ApiController]
[Route("api/attendance/employees/{employeeId:int}/vacation-balances")]
[Authorize(Roles = "Admin,HR")]
public class VacationBalancesController : ControllerBase
{
    private readonly IVacationBalanceService _vacationBalanceService;

    public VacationBalancesController(IVacationBalanceService vacationBalanceService) => _vacationBalanceService = vacationBalanceService;

    /// <summary>List the employee's vacation balances, optionally for one year.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<VacationBalanceDto>>>> GetAll(int employeeId, [FromQuery] int? year, CancellationToken ct)
    {
        var result = await _vacationBalanceService.GetByEmployeeAsync(employeeId, year, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Give the employee one or many balances for a year (all or nothing).</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<List<VacationBalanceDto>>>> Create(int employeeId, [FromBody] CreateVacationBalancesDto dto, CancellationToken ct)
    {
        var result = await _vacationBalanceService.CreateAsync(employeeId, dto, ct);
        return StatusCode(result.StatusCode, result);
    }
}