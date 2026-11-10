using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MassperoTVAPI.Shared.Application;using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Vacations;

namespace MassperoTVAPI.Modules.AttendanceModule.Api.Controllers.Vacations;

[ApiController]
[Route("api/attendance/vacation-types")]
[Authorize(Roles = "Admin,HR")]
public class VacationTypesController : ControllerBase
{
    private readonly IVacationTypeService _vacationTypeService;

    public VacationTypesController(IVacationTypeService vacationTypeService) => _vacationTypeService = vacationTypeService;

    /// <summary>List vacation types ordered by name.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<VacationTypeDto>>>> GetAll(CancellationToken ct)
    {
        var result = await _vacationTypeService.GetAllAsync(ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Create a vacation type.</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<VacationTypeDto>>> Create([FromBody] CreateVacationTypeDto dto, CancellationToken ct)
    {
        var result = await _vacationTypeService.CreateAsync(dto, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Edit a vacation type. Changing DefaultDays does not change existing balances.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<VacationTypeDto>>> Update(int id, [FromBody] UpdateVacationTypeDto dto, CancellationToken ct)
    {
        var result = await _vacationTypeService.UpdateAsync(id, dto, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Soft delete a vacation type that is not used by any request or balance.</summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id, CancellationToken ct)
    {
        var result = await _vacationTypeService.DeleteAsync(id, ct);
        return StatusCode(result.StatusCode, result);
    }
}