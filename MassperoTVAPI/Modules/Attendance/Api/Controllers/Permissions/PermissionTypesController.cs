using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MassperoTVAPI.Shared.Application;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Permissions;

namespace MassperoTVAPI.Modules.AttendanceModule.Api.Controllers.Permissions;

[ApiController]
[Route("api/attendance/permission-types")]
[Authorize(Roles = "Admin,HR")]
public class PermissionTypesController : ControllerBase
{
    private readonly IPermissionTypeService _permissionTypeService;

    public PermissionTypesController(IPermissionTypeService permissionTypeService) => _permissionTypeService = permissionTypeService;

    /// <summary>List permission types ordered by name, optionally filtered by isActive.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PermissionTypeDto>>>> GetAll([FromQuery] bool? isActive, CancellationToken ct)
    {
        var result = await _permissionTypeService.GetAllAsync(isActive, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Create a permission type.</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<PermissionTypeDto>>> Create([FromBody] CreatePermissionTypeDto dto, CancellationToken ct)
    {
        var result = await _permissionTypeService.CreateAsync(dto, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Edit a permission type.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<PermissionTypeDto>>> Update(int id, [FromBody] UpdatePermissionTypeDto dto, CancellationToken ct)
    {
        var result = await _permissionTypeService.UpdateAsync(id, dto, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Soft delete a permission type that no request or balance uses.</summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id, CancellationToken ct)
    {
        var result = await _permissionTypeService.DeleteAsync(id, ct);
        return StatusCode(result.StatusCode, result);
    }
}
