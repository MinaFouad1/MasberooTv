using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Helpers;
using MassperoTVAPI.Core.Interfaces;
using MassperoTVAPI.Core.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MassperoTVAPI.Controllers;

[ApiController]
[Route("api/[controller]")]

public class InterviewsController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public InterviewsController(IUnitOfWork uow) => _uow = uow;

    /// <summary>List all interviews with full details.</summary>
    /// <returns>List of interviews with candidate and type info.</returns>
    [Authorize(Roles = "HR,Admin")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<InterviewDto>>>> GetAll()
    {
        var list = await _uow.Interviews.GetAllWithDetailsAsync();
        var dto = list.Select(x => x.ToDto()); 
        return Ok(ApiResponse<IEnumerable<InterviewDto>>.SuccessResponse(
            dto));
    }

    /// <summary>Get a single interview by ID.</summary>
    /// <param name="id">Interview ID.</param>
    /// <returns>The interview details.</returns>
    [Authorize(Roles = "HR,Admin")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<InterviewDto>>> GetById(int id)
    {
        var entity = await _uow.Interviews.GetByIdWithDetailsAsync(id);
        if (entity is null) return NotFound(ApiResponse<InterviewDto>.NotFoundResponse());
        return Ok(ApiResponse<InterviewDto>.SuccessResponse(entity.ToDto()));
    }

    /// <summary>Get all interviews for a specific candidate.</summary>
    /// <param name="candidateId">Candidate ID.</param>
    /// <returns>List of interviews for the candidate.</returns>
    [Authorize(Roles = "HR,Admin")]
    [HttpGet("by-candidate/{candidateId:int}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<InterviewDto>>>> GetByCandidate(int candidateId)
    {
        var list = await _uow.Interviews.GetByCandidateAsync(candidateId);
        return Ok(ApiResponse<IEnumerable<InterviewDto>>.SuccessResponse(
            list.Select(i => i.ToDto())));
    }

    // ── GET /api/interviews/by-candidate/{candidateId}/type/{typeId} ──────────
    /// <summary>Get interviews for a candidate filtered by type (HR or Technical).</summary>
    /// 
    [Authorize(Roles = "HR,Admin")]
    [HttpGet("by-candidate/{candidateId:int}/type/{typeId:int}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<InterviewDto>>>> GetByCandidateAndType(
        int candidateId, int typeId)
    {
        var list = await _uow.Interviews.GetByCandidateAndTypeAsync(candidateId, typeId);
        return Ok(ApiResponse<IEnumerable<InterviewDto>>.SuccessResponse(
            list.Select(i => i.ToDto())));
    }

    /// <summary>Schedule a new interview for a candidate. HR only.</summary>
    /// <param name="dto">Interview details including candidate, type, grade, and comments.</param>
    /// <returns>The created interview.</returns>
    [Authorize(Roles = "HR")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<InterviewDto>>> Create([FromBody] CreateInterviewDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<InterviewDto>.ErrorResponse("Validation failed."));

        if (!await _uow.Candidates.ExistsAsync(dto.CandidateId))
            return BadRequest(ApiResponse<InterviewDto>.ErrorResponse($"Candidate {dto.CandidateId} not found."));
        if (!await _uow.InterviewTypes.ExistsAsync(dto.TypeId))
            return BadRequest(ApiResponse<InterviewDto>.ErrorResponse($"InterviewType {dto.TypeId} not found."));

        var entity = new Interview
        {
            Grade       = dto.Grade,
            Comments    = dto.Comments,
            CandidateId = dto.CandidateId,
            TypeId      = dto.TypeId
        };

        await _uow.Interviews.AddAsync(entity);
        await _uow.SaveChangesAsync();

        var created = await _uow.Interviews.GetByIdWithDetailsAsync(entity.Id);
        return StatusCode(201, ApiResponse<InterviewDto>.CreatedResponse(created!.ToDto()));
    }

    /// <summary>Update the grade and optional comments for an interview. HR only.</summary>
    /// <param name="id">Interview ID.</param>
    /// <param name="dto">New grade and optional comments.</param>
    /// <returns>The updated interview.</returns>
    [HttpPatch("{id:int}/grade")]
    [Authorize(Roles = "HR")]
    public async Task<ActionResult<ApiResponse<InterviewDto>>> PatchGrade(
        int id, [FromBody] PatchInterviewGradeDto dto)
    {
        var entity = await _uow.Interviews.GetByIdAsync(id);
        if (entity is null) return NotFound(ApiResponse<InterviewDto>.NotFoundResponse());

        entity.Grade    = dto.Grade;
        entity.Comments = dto.Comments ?? entity.Comments;
        _uow.Interviews.Update(entity);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Interviews.GetByIdWithDetailsAsync(entity.Id);
        return Ok(ApiResponse<InterviewDto>.SuccessResponse(updated!.ToDto(), "Grade updated."));
    }

   

    /// <summary>Delete an interview. HR only.</summary>
    /// <param name="id">Interview ID.</param>
    /// <returns>Success status.</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "HR")]
    public async Task<IActionResult> Delete(int id)
    {
        try{


            var entity = await _uow.Interviews.GetByIdAsync(id);
            if (entity is null) return NotFound(ApiResponse<object>.NotFoundResponse());
            _uow.Interviews.Delete(entity);
            await _uow.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse($"Can not delete this one {ex.Message}"));
        }
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Deleted succesffuly"));
    }
}
