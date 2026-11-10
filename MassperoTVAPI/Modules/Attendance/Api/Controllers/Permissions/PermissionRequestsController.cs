using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MassperoTVAPI.Shared.Application;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Permissions;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;

namespace MassperoTVAPI.Modules.AttendanceModule.Api.Controllers.Permissions;

[ApiController]
[Route("api/attendance/permission-requests")]
[Authorize(Roles = "Admin,HR")]
public class PermissionRequestsController : ControllerBase
{
    private readonly IPermissionRequestService _permissionRequestService;

    public PermissionRequestsController(IPermissionRequestService permissionRequestService) => _permissionRequestService = permissionRequestService;

    /// <summary>List permission requests (including Cancelled), newest first.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PermissionRequestDto>>>> GetAll(
        [FromQuery] int? employeeId, [FromQuery] int? permissionTypeId, [FromQuery] RequestStatus? status,
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
    {
        var result = await _permissionRequestService.GetAllAsync(employeeId, permissionTypeId, status, from, to, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Create a permission request (approved automatically).</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<PermissionRequestDto>>> Create([FromBody] CreatePermissionRequestDto dto, CancellationToken ct)
    {
        var result = await _permissionRequestService.CreateAsync(dto, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Withdraw an approved permission request (status Cancelled) and recompute its month.</summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<PermissionRequestDto>>> Withdraw(int id, CancellationToken ct)
    {
        var result = await _permissionRequestService.WithdrawAsync(id, ct);
        return StatusCode(result.StatusCode, result);
    }
}
