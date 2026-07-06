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
[Authorize(Roles = "Admin,HR")]
public class IssuesController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public IssuesController(IUnitOfWork uow) => _uow = uow;

    /// <summary>List all issues with optional filters by process ID, date, or resolved status.</summary>
    /// <param name="processId">Optional. Filter by process ID.</param>
    /// <param name="date">Optional. Filter by issue date.</param>
    /// <param name="resolved">Optional. Filter by resolved/unresolved status.</param>
    /// <returns>Filtered list of issues.</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<IssueDetailDto>>>> GetAll(
        [FromQuery] int? processId,
        [FromQuery] DateTime? date,
        [FromQuery] bool? resolved)
    {
        var list = await _uow.Issues.GetAllWithDetailsAsync(processId, date, resolved);
        return Ok(ApiResponse<IEnumerable<IssueDetailDto>>.SuccessResponse(
            list.Select(i => i.ToDto())));
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

        var entity = new Issue
        {
            Name = dto.Name,
            ProcessId = dto.ProcessId,
            Resolved = dto.Resolved ?? false,
            Date = dto.Date ?? DateTime.UtcNow
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
        entity.Resolved  = dto.Resolved ?? entity.Resolved;
        entity.Date      = dto.Date ?? entity.Date;

        _uow.Issues.Update(entity);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Issues.GetByIdWithDetailsAsync(entity.Id);
        return Ok(ApiResponse<IssueDetailDto>.SuccessResponse(updated!.ToDto()));
    }

    /// <summary>Toggle the resolved status of an issue.</summary>
    /// <param name="id">Issue ID.</param>
    /// <param name="dto">New resolved state.</param>
    /// <returns>The updated issue.</returns>
    [HttpPatch("{id:int}/resolved")]
    public async Task<ActionResult<ApiResponse<IssueDetailDto>>> PatchResolved(
        int id, [FromBody] PatchIssueResolvedDto dto)
    {
        var entity = await _uow.Issues.GetByIdAsync(id);
        if (entity is null) return NotFound(ApiResponse<IssueDetailDto>.NotFoundResponse());

        entity.Resolved = dto.Resolved;
        _uow.Issues.Update(entity);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Issues.GetByIdWithDetailsAsync(entity.Id);
        return Ok(ApiResponse<IssueDetailDto>.SuccessResponse(updated!.ToDto(), "Issue resolved status updated."));
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
 
