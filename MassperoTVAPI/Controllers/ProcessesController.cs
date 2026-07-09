using System.Security.Claims;
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
[Authorize]
public class ProcessesController : ControllerBase
{
    private const string InitialProcessStatusName = "Pending";

    private readonly IUnitOfWork _uow;

    public ProcessesController(IUnitOfWork uow) => _uow = uow;

    /// <summary>List all processes with full details including dependencies and issues.</summary>
    /// <returns>List of processes.</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProcessDto>>>> GetAll()
    {
        var list = await _uow.Processes.GetAllWithDetailsAsync();
        return Ok(ApiResponse<IEnumerable<ProcessDto>>.SuccessResponse(
            list.Select(p => p.ToDto())));
    }

    /// <summary>Get a single process by ID with its dependencies and issues.</summary>
    /// <param name="id">Process ID.</param>
    /// <returns>The process details.</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProcessDto>>> GetById(int id)
    {
        var entity = await _uow.Processes.GetWithDependenciesAsync(id);
        if (entity is null) return NotFound(ApiResponse<ProcessDto>.NotFoundResponse());
        return Ok(ApiResponse<ProcessDto>.SuccessResponse(entity.ToDto()));
    }

    /// <summary>Create a new process with Pending status by default. Admin or HR required.</summary>
    /// <param name="dto">Process data including name, dates, and phase ID.</param>
    /// <returns>The created process.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<ApiResponse<ProcessDto>>> Create([FromBody] CreateProcessDto dto)
    {
        if (!await _uow.Phases.ExistsAsync(dto.PhaseId))
            return BadRequest(ApiResponse<ProcessDto>.ErrorResponse($"Phase {dto.PhaseId} not found."));

        var pendingStatus = await GetProcessStatusByNameAsync(InitialProcessStatusName);
        if (pendingStatus is null)
            return BadRequest(ApiResponse<ProcessDto>.ErrorResponse($"ProcessStatus '{InitialProcessStatusName}' not found."));

        var entity = new Process
        {
            Name            = dto.Name,
            StartDate       = dto.StartDate,
            EndDate         = dto.EndDate,
            PhaseId         = dto.PhaseId,
            ProcessStatusId = pendingStatus.Id
        };

        await _uow.Processes.AddAsync(entity);
        await _uow.SaveChangesAsync();

        var created = await _uow.Processes.GetWithDependenciesAsync(entity.Id);
        return StatusCode(201, ApiResponse<ProcessDto>.CreatedResponse(created!.ToDto()));
    }

    /// <summary>Update a process including its phase and process status. Admin or HR required.</summary>
    /// <param name="id">Process ID.</param>
    /// <param name="dto">Updated process data.</param>
    /// <returns>The updated process.</returns>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<ApiResponse<ProcessDto>>> Update(int id, [FromBody] UpdateProcessDto dto)
    {
        var entity = await _uow.Processes.GetWithDependenciesAsync(id);
        if (entity is null) return NotFound(ApiResponse<ProcessDto>.NotFoundResponse());

        if (!await _uow.Phases.ExistsAsync(dto.PhaseId))
            return BadRequest(ApiResponse<ProcessDto>.ErrorResponse($"Phase {dto.PhaseId} not found."));

        entity.Name            = dto.Name;
        entity.StartDate       = dto.StartDate;
        entity.EndDate         = dto.EndDate;
        entity.PhaseId         = dto.PhaseId;
        entity.ProcessStatusId = dto.statusId;

        _uow.Processes.Update(entity);
        await _uow.SaveChangesAsync();

        return Ok(ApiResponse<ProcessDto>.SuccessResponse(entity.ToDto()));
    }

    /// <summary>Delete a process. Admin only.</summary>
    /// <param name="id">Process ID.</param>
    /// <returns>Success status.</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var entity = await _uow.Processes.GetByIdAsync(id);
            if (entity is null) return NotFound(ApiResponse<object>.NotFoundResponse());
            _uow.Processes.Delete(entity);
            await _uow.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse($"Can not delete this one {ex.Message}"));
        }
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Deleted succesffuly"));
    }

    /// <summary>Replace all dependencies for a process. Pass an empty list to clear dependencies. Admin or HR required.</summary>
    /// <param name="id">Process ID.</param>
    /// <param name="dto">List of process IDs this process depends on.</param>
    /// <returns>The updated process with its new dependencies.</returns>
    [HttpPatch("{id:int}/dependencies")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<ApiResponse<ProcessDto>>> SetDependencies(
        int id, [FromBody] ProcessDependencyDto dto)
    {
        var entity = await _uow.Processes.GetWithDependenciesAsync(id);
        if (entity is null) return NotFound(ApiResponse<ProcessDto>.NotFoundResponse());

        var dependsOnIds = dto.DependsOnProcessIds.Distinct().ToList();

        if (dependsOnIds.Contains(id))
            return BadRequest(ApiResponse<ProcessDto>.ErrorResponse("A process cannot depend on itself."));

        foreach (var depId in dependsOnIds)
            if (!await _uow.Processes.ExistsAsync(depId))
                return BadRequest(ApiResponse<ProcessDto>.ErrorResponse($"Process {depId} not found."));

        // Remove existing then add new — done via ProcessDependencies DbSet
        // We still need direct context access here for the junction table
        // Route through a dedicated method in the repo to keep controller clean
        if (entity.Dependencies is not null)
            entity.Dependencies.Clear();

        entity.Dependencies = dependsOnIds.Select(depId => new ProcessDependency
        {
            ProcessId          = id,
            DependsOnProcessId = depId
        }).ToList();

        _uow.Processes.Update(entity);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Processes.GetWithDependenciesAsync(id);
        return Ok(ApiResponse<ProcessDto>.SuccessResponse(updated!.ToDto()));
    }

    /// <summary>Remove a single dependency from a process. Admin or HR required.</summary>
    /// <param name="id">Process ID.</param>
    /// <param name="dependsOnId">The dependency process ID to remove.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id:int}/dependencies/{dependsOnId:int}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> RemoveDependency(int id, int dependsOnId)
    {
        var entity = await _uow.Processes.GetWithDependenciesAsync(id);
        if (entity is null) return NotFound(ApiResponse<object>.NotFoundResponse());

        var dep = entity.Dependencies?.FirstOrDefault(d => d.DependsOnProcessId == dependsOnId);
        if (dep is null)
            return NotFound(ApiResponse<object>.ErrorResponse(
                $"No dependency from process {id} to process {dependsOnId} found."));

        entity.Dependencies!.Remove(dep);
        _uow.Processes.Update(entity);
        await _uow.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>Change the status of a process.</summary>
    /// <param name="id">Process ID.</param>
    /// <param name="dto">New status ID.</param>
    /// <returns>The updated process.</returns>
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<ApiResponse<ProcessDto>>> ChangeStatus(int id, [FromBody] ChangeProcessStatusDto dto)
    {
        var entity = await _uow.Processes.GetWithDependenciesAsync(id);
        if (entity is null) return NotFound(ApiResponse<ProcessDto>.NotFoundResponse("Process not found."));

        var status = await _uow.ProcessStatuses.GetByIdAsync(dto.StatusId);
        if (status is null)
            return BadRequest(ApiResponse<ProcessDto>.ErrorResponse($"ProcessStatus {dto.StatusId} not found."));

        entity.ProcessStatusId = dto.StatusId;

        _uow.Processes.Update(entity);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Processes.GetWithDependenciesAsync(id);
        return Ok(ApiResponse<ProcessDto>.SuccessResponse(updated!.ToDto(), "Status updated successfully."));
    }

    /// <summary>Get all available process statuses.</summary>
    /// <returns>List of process statuses.</returns>
    [HttpGet("statuses")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProcessStatusDto>>>> GetAllStatuses()
    {
        var statuses = await _uow.ProcessStatuses.GetAllAsync();
        var result = statuses.Select(s => new ProcessStatusDto(s.Id, s.Name, s.Percentage));
        return Ok(ApiResponse<IEnumerable<ProcessStatusDto>>.SuccessResponse(result));
    }

    private async Task<ProcessStatus?> GetProcessStatusByNameAsync(string name)
        => (await _uow.ProcessStatuses.GetAllAsync())
            .FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
}
 
