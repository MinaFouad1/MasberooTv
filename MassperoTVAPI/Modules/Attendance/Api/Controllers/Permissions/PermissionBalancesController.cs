using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MassperoTVAPI.Shared.Application;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Permissions;

namespace MassperoTVAPI.Modules.AttendanceModule.Api.Controllers.Permissions;

[ApiController]
[Route("api/attendance/employees/{employeeId:int}/permission-balances")]
[Authorize(Roles = "Admin,HR")]
public class PermissionBalancesController : ControllerBase
{
    private readonly IPermissionBalanceService _permissionBalanceService;

    public PermissionBalancesController(IPermissionBalanceService permissionBalanceService) => _permissionBalanceService = permissionBalanceService;

    /// <summary>List the employee's permission balances, optionally for one year and/or month.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PermissionBalanceDto>>>> GetAll(int employeeId, [FromQuery] int? year, [FromQuery] int? month, CancellationToken ct)
    {
        var result = await _permissionBalanceService.GetByEmployeeAsync(employeeId, year, month, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Assign balances (types x months) to the employee, all or nothing.</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<List<PermissionBalanceDto>>>> Create(int employeeId, [FromBody] CreatePermissionBalancesDto dto, CancellationToken ct)
    {
        var result = await _permissionBalanceService.CreateAsync(employeeId, dto, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Change the allowance of one existing balance and recompute its month.</summary>
    [HttpPut]
    public async Task<ActionResult<ApiResponse<PermissionBalanceDto>>> Update(int employeeId, [FromBody] UpdatePermissionBalanceDto dto, CancellationToken ct)
    {
        var result = await _permissionBalanceService.UpdateAsync(employeeId, dto, ct);
        return StatusCode(result.StatusCode, result);
    }
}
