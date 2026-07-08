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
