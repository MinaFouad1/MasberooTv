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
    private readonly IEmailService _emailService;

    public InterviewsController(IUnitOfWork uow, IEmailService emailService)
    {
        _uow = uow;
        _emailService = emailService;
    }

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

    /// <summary>Creates an interview schedule and sends the details to the candidate by email.</summary>
    [Authorize(Roles = "HR,Admin")]
    [HttpPost("schedule")]
    public async Task<ActionResult<ApiResponse<InterviewDto>>> Schedule(
        [FromBody] ScheduleInterviewDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<InterviewDto>.ErrorResponse("Validation failed."));

        if (dto.InterviewDate.Date < DateTime.UtcNow.Date)
            return BadRequest(ApiResponse<InterviewDto>.ErrorResponse(
                "InterviewDate cannot be in the past."));

        if (dto.EndTime <= dto.StartTime)
            return BadRequest(ApiResponse<InterviewDto>.ErrorResponse(
                "EndTime must be later than StartTime."));

        var candidate = await _uow.Candidates.GetByIdAsync(dto.CandidateId);
        if (candidate is null)
            return NotFound(ApiResponse<InterviewDto>.NotFoundResponse(
                $"Candidate {dto.CandidateId} not found."));

        if (string.IsNullOrWhiteSpace(candidate.Email))
            return BadRequest(ApiResponse<InterviewDto>.ErrorResponse(
                "The candidate does not have an email address."));

        var interviewType = await _uow.InterviewTypes.GetByIdAsync(dto.TypeId);
        if (interviewType is null)
            return NotFound(ApiResponse<InterviewDto>.NotFoundResponse(
                $"Interview type {dto.TypeId} not found."));

        var interview = new Interview
        {
            CandidateId = dto.CandidateId,
            TypeId = dto.TypeId,
            InterviewMode = dto.InterviewMode.Trim(),
            InterviewDate = dto.InterviewDate.Date,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            CreatedAt = DateTime.UtcNow
        };

        var candidateName = System.Net.WebUtility.HtmlEncode(candidate.Name);
        var typeName = System.Net.WebUtility.HtmlEncode(interviewType.Name);
        var mode = System.Net.WebUtility.HtmlEncode(interview.InterviewMode);
        var date = interview.InterviewDate.Value.ToString("dddd, dd MMMM yyyy");
        var start = interview.StartTime.Value.ToString("HH:mm");
        var end = interview.EndTime.Value.ToString("HH:mm");

        try
        {
            await _emailService.SendAsync(
                candidate.Email,
                candidate.Name,
                $"Interview invitation - {interviewType.Name}",
                $"<p>Dear {candidateName},</p>" +
                $"<p>You are invited to a <strong>{typeName}</strong> interview.</p>" +
                $"<ul><li>Mode: {mode}</li><li>Date: {date}</li>" +
                $"<li>Time: {start} - {end}</li></ul>");
        }
        catch
        {
            return StatusCode(500, ApiResponse<InterviewDto>.ErrorResponse(
                "The interview email could not be sent. No interview was scheduled."));
        }

        await _uow.Interviews.AddAsync(interview);
        await _uow.SaveChangesAsync();

        var created = await _uow.Interviews.GetByIdWithDetailsAsync(interview.Id);
        return StatusCode(201, ApiResponse<InterviewDto>.CreatedResponse(
            created!.ToDto(), "Interview scheduled and email sent."));
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
