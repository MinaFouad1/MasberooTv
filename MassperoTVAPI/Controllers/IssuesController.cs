using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Helpers;
using MassperoTVAPI.Core.Interfaces;
using MassperoTVAPI.Core.Mappers;
using MassperoTVAPI.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MassperoTVAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,HR")]
public class IssuesController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public IssuesController(IUnitOfWork uow) => _uow = uow;

    /// <summary>List all issues with optional filters by name, priority, process ID, date, or resolved status.</summary>
    /// <param name="name">Optional. Filter by issue/risk name.</param>
    /// <param name="priority">Optional. Filter by priority.</param>
    /// <param name="processId">Optional. Filter by process ID.</param>
    /// <param name="date">Optional. Filter by issue date.</param>
    /// <param name="resolved">Optional. Filter by resolved/unresolved status.</param>
    /// <returns>Filtered list of risks.</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<RiskResponseDto>>>> GetAll(
        [FromQuery] string? name,
        [FromQuery] IssuePriority? priority,
        [FromQuery] int? processId,
        [FromQuery] DateTime? date,
        [FromQuery] IssueStatus? status)
    {
        var list = await _uow.Issues.GetAllWithDetailsAsync(processId, date, status, name, priority);
        return Ok(ApiResponse<IEnumerable<RiskResponseDto>>.SuccessResponse(
            list.Select(i => i.ToRiskResponseDto())));
    }

    /// <summary>Get statistics for all risks/issues including total count and percentages by priority.</summary>
    /// <returns>Statistics object with counts and percentages.</returns>
    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<RiskStatisticsDto>>> GetStatistics()
    {
        var issues = await _uow.Issues.GetAllWithDetailsAsync();
        var total = issues.Count();
        
        var criticalCount = issues.Count(i => i.Priority == IssuePriority.Critical);
        var highCount = issues.Count(i => i.Priority == IssuePriority.High);
        var mediumCount = issues.Count(i => i.Priority == IssuePriority.Medium);
        var lowCount = issues.Count(i => i.Priority == IssuePriority.Low);

        var stats = new RiskStatisticsDto(
            total,
            new RiskPriorityStatDto(criticalCount, total > 0 ? (decimal)criticalCount / total * 100 : 0),
            new RiskPriorityStatDto(highCount, total > 0 ? (decimal)highCount / total * 100 : 0),
            new RiskPriorityStatDto(mediumCount, total > 0 ? (decimal)mediumCount / total * 100 : 0),
            new RiskPriorityStatDto(lowCount, total > 0 ? (decimal)lowCount / total * 100 : 0)
        );

        return Ok(ApiResponse<RiskStatisticsDto>.SuccessResponse(stats));
    }

    /// <summary>Get top 5 unresolved issues ordered by priority (Critical first).</summary>
    /// <returns>List of top 5 risk issues.</returns>
    [HttpGet("top-risks")]
    public async Task<ActionResult<ApiResponse<IEnumerable<RiskResponseDto>>>> GetTopRisks()
    {
        var topRisks = await _uow.Issues.GetTopRisksAsync(5);
        return Ok(ApiResponse<IEnumerable<RiskResponseDto>>.SuccessResponse(
            topRisks.Select(i => i.ToRiskResponseDto())));
    }

    /// <summary>Get a single issue by ID.</summary>
    /// <param name="id">Issue ID.</param>
    /// <returns>The issue details.</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<IssueDetailDto>>> GetById(int id)
    {
        var entity = await _uow.Issues.GetByIdWithDetailsAsync(id);
        if (entity is null) return NotFound(ApiResponse<IssueDetailDto>.NotFoundResponse());
        return Ok(ApiResponse<IssueDetailDto>.SuccessResponse(entity.ToDto()));
    }

    /// <summary>Get all issues associated with a specific process.</summary>
    /// <param name="processId">Process ID.</param>
    /// <returns>List of issues for the process.</returns>
    [HttpGet("by-process/{processId:int}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<IssueDetailDto>>>> GetByProcess(int processId)
    {
        var list = await _uow.Issues.GetByProcessAsync(processId);
        return Ok(ApiResponse<IEnumerable<IssueDetailDto>>.SuccessResponse(
            list.Select(i => i.ToDto())));
    }

    /// <summary>Create a new issue for a process.</summary>
    /// <param name="dto">Issue details including name, process ID, and optional resolved/date.</param>
    /// <returns>The created issue.</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<IssueDetailDto>>> Create([FromBody] CreateIssueDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<IssueDetailDto>.ErrorResponse("Validation failed."));

        if (!await _uow.Processes.ExistsAsync(dto.ProcessId))
            return BadRequest(ApiResponse<IssueDetailDto>.ErrorResponse($"Process {dto.ProcessId} not found."));

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var entity = new Issue
        {
            Name = dto.Name,
            ProcessId = dto.ProcessId,
            Date = dto.Date ?? DateTime.UtcNow,
            DueDate = dto.DueDate,
            Priority = dto.Priority,
            Status = dto.Status ?? IssueStatus.Pending,
            CreatedByUserId = userId
        };
        await _uow.Issues.AddAsync(entity);
        await _uow.SaveChangesAsync();

        var created = await _uow.Issues.GetByIdWithDetailsAsync(entity.Id);
        return StatusCode(201, ApiResponse<IssueDetailDto>.CreatedResponse(created!.ToDto()));
    }

    /// <summary>Update an existing issue.</summary>
    /// <param name="id">Issue ID.</param>
    /// <param name="dto">Updated issue data.</param>
    /// <returns>The updated issue.</returns>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<IssueDetailDto>>> Update(int id, [FromBody] UpdateIssueDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<IssueDetailDto>.ErrorResponse("Validation failed."));

        var entity = await _uow.Issues.GetByIdAsync(id);
        if (entity is null) return NotFound(ApiResponse<IssueDetailDto>.NotFoundResponse());

        if (!await _uow.Processes.ExistsAsync(dto.ProcessId))
            return BadRequest(ApiResponse<IssueDetailDto>.ErrorResponse($"Process {dto.ProcessId} not found."));

        entity.Name      = dto.Name;
        entity.ProcessId = dto.ProcessId;
        entity.Date      = dto.Date ?? entity.Date;
        entity.DueDate   = dto.DueDate ?? entity.DueDate;
        entity.Priority  = dto.Priority ?? entity.Priority;
        if (dto.Status.HasValue)
        {
            entity.Status = dto.Status.Value;
        }

        _uow.Issues.Update(entity);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Issues.GetByIdWithDetailsAsync(entity.Id);
        return Ok(ApiResponse<IssueDetailDto>.SuccessResponse(updated!.ToDto()));
    }

    /// <summary>Change the status of an issue.</summary>
    /// <param name="id">Issue ID.</param>
    /// <param name="dto">The new status.</param>
    /// <returns>The updated issue.</returns>
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<ApiResponse<IssueDetailDto>>> ChangeStatus(int id, [FromBody] PatchIssueStatusDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<IssueDetailDto>.ErrorResponse("Validation failed."));

        var entity = await _uow.Issues.GetByIdAsync(id);
        if (entity is null) return NotFound(ApiResponse<IssueDetailDto>.NotFoundResponse());

        entity.Status = dto.Status;
        _uow.Issues.Update(entity);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Issues.GetByIdWithDetailsAsync(entity.Id);
        return Ok(ApiResponse<IssueDetailDto>.SuccessResponse(updated!.ToDto()));
    }

    /// <summary>Delete an issue. Admin only.</summary>
    /// <param name="id">Issue ID.</param>
    /// <returns>Success status.</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var entity = await _uow.Issues.GetByIdAsync(id);
            if (entity is null) return NotFound(ApiResponse<object>.NotFoundResponse());
            _uow.Issues.Delete(entity);
            await _uow.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse($"Can not delete this one {ex.Message}"));
        }
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Deleted succesffuly"));
    }
}
 
