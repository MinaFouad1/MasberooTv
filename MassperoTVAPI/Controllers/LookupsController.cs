using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Enums;
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

    /// <summary>List all offer statuses (Pending = 1, Accepted = 2, Declined = 3, Expired = 4).</summary>
    [HttpGet("offer-statuses")]
    public ActionResult<ApiResponse<IEnumerable<OfferStatusDto>>> GetOfferStatuses()
    {
        var statuses = Enum.GetValues<OfferStatus>()
            .Select(s => new OfferStatusDto((int)s, s.ToString()))
            .ToList();

        return Ok(ApiResponse<IEnumerable<OfferStatusDto>>.SuccessResponse(statuses));
    }

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

   
}
