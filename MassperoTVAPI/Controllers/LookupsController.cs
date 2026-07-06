using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Helpers;
using MassperoTVAPI.Core.Interfaces;
using MassperoTVAPI.Core.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MassperoTVAPI.Controllers;

/// <summary>
/// CRUD for all lookup / reference tables.
/// Read endpoints are open to authenticated users; write endpoints require Admin.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LookupsController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public LookupsController(IUnitOfWork uow) => _uow = uow;

    // ════════════════════════════════════════════════════════════
    // CANDIDATE STATUS
    // ════════════════════════════════════════════════════════════

    /// <summary>List all candidate application statuses (e.g. Under Vetting, Approved, Rejected).</summary>
    [HttpGet("statuses")]
    public async Task<ActionResult<ApiResponse<IEnumerable<StatusDto>>>> GetStatuses()
    {
        var list = await _uow.Statuses.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<StatusDto>>.SuccessResponse(list.Select(s => s.ToDto())));
    }

    /// <summary>Get a single application status by ID.</summary>
    [HttpGet("statuses/{id:int}")]
    public async Task<ActionResult<ApiResponse<StatusDto>>> GetStatus(int id)
    {
        var entity = await _uow.Statuses.GetByIdAsync(id);
        if (entity is null) return NotFound(ApiResponse<StatusDto>.NotFoundResponse());
        return Ok(ApiResponse<StatusDto>.SuccessResponse(entity.ToDto()));
    }

    /// <summary>Create a new application status. Admin only.</summary>
    [HttpPost("statuses")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<StatusDto>>> CreateStatus([FromBody] CreateStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<StatusDto>.ErrorResponse("Validation failed."));
        var entity = new Status { Name = dto.Name };
        await _uow.Statuses.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return StatusCode(201, ApiResponse<StatusDto>.CreatedResponse(entity.ToDto()));
    }

    /// <summary>Update an application status name. Admin only.</summary>
    [HttpPut("statuses/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<StatusDto>>> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<StatusDto>.ErrorResponse("Validation failed."));
        var entity = await _uow.Statuses.GetByIdAsync(id);
        if (entity is null) return NotFound(ApiResponse<StatusDto>.NotFoundResponse());
        entity.Name = dto.Name;
        _uow.Statuses.Update(entity);
        await _uow.SaveChangesAsync();
        return Ok(ApiResponse<StatusDto>.SuccessResponse(entity.ToDto()));
    }

    /// <summary>Delete an application status. Admin only.</summary>
    [HttpDelete("statuses/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteStatus(int id)
    {
        try
        {
            var entity = await _uow.Statuses.GetByIdAsync(id);
            if (entity is null) return NotFound(ApiResponse<object>.NotFoundResponse());
            _uow.Statuses.Delete(entity);
            await _uow.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse($"Can not delete this one {ex.Message}"));
        }
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Deleted succesffuly"));
    }

    // ════════════════════════════════════════════════════════════
    // SECURITY CLEARANCE STATUS
    // ════════════════════════════════════════════════════════════

    /// <summary>List all security clearance statuses (e.g. In Check, Cleared, Denied).</summary>
    [HttpGet("security-clearances")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SecurityClearanceStatusDto>>>> GetSecurityClearances()
    {
        var list = await _uow.SecurityClearances.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<SecurityClearanceStatusDto>>.SuccessResponse(list.Select(s => s.ToDto())));
    }

    /// <summary>Get a single security clearance status by ID.</summary>
    [HttpGet("security-clearances/{id:int}")]
    public async Task<ActionResult<ApiResponse<SecurityClearanceStatusDto>>> GetSecurityClearance(int id)
    {
        var entity = await _uow.SecurityClearances.GetByIdAsync(id);
        if (entity is null) return NotFound(ApiResponse<SecurityClearanceStatusDto>.NotFoundResponse());
        return Ok(ApiResponse<SecurityClearanceStatusDto>.SuccessResponse(entity.ToDto()));
    }

    /// <summary>Create a new security clearance status. Admin only.</summary>
    [HttpPost("security-clearances")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<SecurityClearanceStatusDto>>> CreateSecurityClearance(
        [FromBody] CreateSecurityClearanceStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<SecurityClearanceStatusDto>.ErrorResponse("Validation failed."));
        var entity = new SecurityClearanceStatus { Name = dto.Name };
        await _uow.SecurityClearances.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return StatusCode(201, ApiResponse<SecurityClearanceStatusDto>.CreatedResponse(entity.ToDto()));
    }

    /// <summary>Update a security clearance status name. Admin only.</summary>
    [HttpPut("security-clearances/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<SecurityClearanceStatusDto>>> UpdateSecurityClearance(
        int id, [FromBody] UpdateSecurityClearanceStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<SecurityClearanceStatusDto>.ErrorResponse("Validation failed."));
        var entity = await _uow.SecurityClearances.GetByIdAsync(id);
        if (entity is null) return NotFound(ApiResponse<SecurityClearanceStatusDto>.NotFoundResponse());
        entity.Name = dto.Name;
        _uow.SecurityClearances.Update(entity);
        await _uow.SaveChangesAsync();
        return Ok(ApiResponse<SecurityClearanceStatusDto>.SuccessResponse(entity.ToDto()));
    }

    /// <summary>Delete a security clearance status. Admin only.</summary>
    [HttpDelete("security-clearances/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSecurityClearance(int id)
    {
        try
        {
            var entity = await _uow.SecurityClearances.GetByIdAsync(id);
            if (entity is null) return NotFound(ApiResponse<object>.NotFoundResponse());
            _uow.SecurityClearances.Delete(entity);
            await _uow.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse($"Can not delete this one {ex.Message}"));
        }
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Deleted succesffuly"));
    }

    // ════════════════════════════════════════════════════════════
    // INTERVIEW TYPES
    // ════════════════════════════════════════════════════════════

    /// <summary>List all interview types (e.g. HR, Technical).</summary>
    [HttpGet("interview-types")]
    public async Task<ActionResult<ApiResponse<IEnumerable<InterviewTypeDto>>>> GetInterviewTypes()
    {
        var list = await _uow.InterviewTypes.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<InterviewTypeDto>>.SuccessResponse(list.Select(t => t.ToDto())));
    }

    /// <summary>Get a single interview type by ID.</summary>
    [HttpGet("interview-types/{id:int}")]
    public async Task<ActionResult<ApiResponse<InterviewTypeDto>>> GetInterviewType(int id)
    {

        var entity = await _uow.InterviewTypes.GetByIdAsync(id);
        if (entity is null) return NotFound(ApiResponse<InterviewTypeDto>.NotFoundResponse());
        return Ok(ApiResponse<InterviewTypeDto>.SuccessResponse(entity.ToDto()));
    }

    /// <summary>Create a new interview type. Admin only.</summary>
    [HttpPost("interview-types")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<InterviewTypeDto>>> CreateInterviewType(
        [FromBody] CreateInterviewTypeDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<InterviewTypeDto>.ErrorResponse("Validation failed."));
        var entity = new InterviewType { Name = dto.Name };
        await _uow.InterviewTypes.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return StatusCode(201, ApiResponse<InterviewTypeDto>.CreatedResponse(entity.ToDto()));
    }

    /// <summary>Update an interview type name. Admin only.</summary>
    [HttpPut("interview-types/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<InterviewTypeDto>>> UpdateInterviewType(
        int id, [FromBody] UpdateInterviewTypeDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<InterviewTypeDto>.ErrorResponse("Validation failed."));
        var entity = await _uow.InterviewTypes.GetByIdAsync(id);
        if (entity is null) return NotFound(ApiResponse<InterviewTypeDto>.NotFoundResponse());
        entity.Name = dto.Name;
        _uow.InterviewTypes.Update(entity);
        await _uow.SaveChangesAsync();
        return Ok(ApiResponse<InterviewTypeDto>.SuccessResponse(entity.ToDto()));
    }

    /// <summary>Delete an interview type. Admin only.</summary>
    [HttpDelete("interview-types/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteInterviewType(int id)
    {
        try
        {
            var entity = await _uow.InterviewTypes.GetByIdAsync(id);
            if (entity is null) return NotFound(ApiResponse<object>.NotFoundResponse());
            _uow.InterviewTypes.Delete(entity);
            await _uow.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse($"Can not delete this one {ex.Message}"));
        }
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Deleted succesffuly"));
    }

    // ════════════════════════════════════════════════════════════
    // PHASES
    // ════════════════════════════════════════════════════════════

    /// <summary>List all process phases.</summary>
    [HttpGet("phases")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PhaseDto>>>> GetPhases()
    {
        var list = await _uow.Phases.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<PhaseDto>>.SuccessResponse(list.Select(p => p.ToDto())));
    }

    /// <summary>Get a single phase by ID.</summary>
    [HttpGet("phases/{id:int}")]
    public async Task<ActionResult<ApiResponse<PhaseDto>>> GetPhase(int id)
    {
        var entity = await _uow.Phases.GetByIdAsync(id);
        if (entity is null) return NotFound(ApiResponse<PhaseDto>.NotFoundResponse());
        return Ok(ApiResponse<PhaseDto>.SuccessResponse(entity.ToDto()));
    }

    /// <summary>Create a new phase. Admin only.</summary>
    [HttpPost("phases")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PhaseDto>>> CreatePhase([FromBody] CreatePhaseDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<PhaseDto>.ErrorResponse("Validation failed."));
        var entity = new Phase { Name = dto.Name };
        await _uow.Phases.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return StatusCode(201, ApiResponse<PhaseDto>.CreatedResponse(entity.ToDto()));
    }

    /// <summary>Update a phase name. Admin only.</summary>
    [HttpPut("phases/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PhaseDto>>> UpdatePhase(int id, [FromBody] UpdatePhaseDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<PhaseDto>.ErrorResponse("Validation failed."));
        var entity = await _uow.Phases.GetByIdAsync(id);
        if (entity is null) return NotFound(ApiResponse<PhaseDto>.NotFoundResponse());
        entity.Name = dto.Name;
        _uow.Phases.Update(entity);
        await _uow.SaveChangesAsync();
        return Ok(ApiResponse<PhaseDto>.SuccessResponse(entity.ToDto()));
    }

    /// <summary>Delete a phase. Admin only.</summary>
    [HttpDelete("phases/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePhase(int id)
    {
        try
        {
            var entity = await _uow.Phases.GetByIdAsync(id);
            if (entity is null) return NotFound(ApiResponse<object>.NotFoundResponse());
            _uow.Phases.Delete(entity);
            await _uow.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse($"Can not delete this one {ex.Message}"));
        }
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Deleted succesffuly"));
    }

    // ════════════════════════════════════════════════════════════
    // PROCESS STATUSES
    // ════════════════════════════════════════════════════════════

    /// <summary>List all process statuses (Pending, InProgress, Completed, Holded).</summary>
    [HttpGet("process-statuses")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProcessStatusDto>>>> GetProcessStatuses()
    {
        var list = await _uow.ProcessStatuses.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<ProcessStatusDto>>.SuccessResponse(list.Select(ps => ps.ToDto())));
    }

    /// <summary>Get a single process status by ID.</summary>
    [HttpGet("process-statuses/{id:int}")]
    public async Task<ActionResult<ApiResponse<ProcessStatusDto>>> GetProcessStatus(int id)
    {
        var entity = await _uow.ProcessStatuses.GetByIdAsync(id);
        if (entity is null) return NotFound(ApiResponse<ProcessStatusDto>.NotFoundResponse());
        return Ok(ApiResponse<ProcessStatusDto>.SuccessResponse(entity.ToDto()));
    }

    /// <summary>Create a new process status with a completion percentage. Admin only.</summary>
    [HttpPost("process-statuses")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ProcessStatusDto>>> CreateProcessStatus(
        [FromBody] CreateProcessStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<ProcessStatusDto>.ErrorResponse("Validation failed."));
        var entity = new ProcessStatus { Name = dto.Name, Percentage = dto.Percentage };
        await _uow.ProcessStatuses.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return StatusCode(201, ApiResponse<ProcessStatusDto>.CreatedResponse(entity.ToDto()));
    }

    /// <summary>Update a process status name and percentage. Admin only.</summary>
    [HttpPut("process-statuses/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ProcessStatusDto>>> UpdateProcessStatus(
        int id, [FromBody] UpdateProcessStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<ProcessStatusDto>.ErrorResponse("Validation failed."));
        var entity = await _uow.ProcessStatuses.GetByIdAsync(id);
        if (entity is null) return NotFound(ApiResponse<ProcessStatusDto>.NotFoundResponse());
        entity.Name       = dto.Name;
        entity.Percentage = dto.Percentage;
        _uow.ProcessStatuses.Update(entity);
        await _uow.SaveChangesAsync();
        return Ok(ApiResponse<ProcessStatusDto>.SuccessResponse(entity.ToDto()));
    }
     
    /// <summary>Delete a process status. Admin only.</summary>
    [HttpDelete("process-statuses/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProcessStatus(int id)
    {
        try
        {
            var entity = await _uow.ProcessStatuses.GetByIdAsync(id);
            if (entity is null) return NotFound(ApiResponse<object>.NotFoundResponse());
            _uow.ProcessStatuses.Delete(entity);
            await _uow.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse($"Can not delete this one {ex.Message}"));
        }
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Deleted succesffuly"));
    }
}
