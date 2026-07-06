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
[Authorize(Roles = "Admin")]
public class PhasesController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public PhasesController(IUnitOfWork uow) => _uow = uow;

    /// <summary>Create a new phase. Admin only.</summary>
    /// <param name="dto">Phase name.</param>
    /// <returns>The created phase.</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<PhaseWithProcessesDto>>> Create([FromBody] CreatePhaseDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<PhaseWithProcessesDto>.ErrorResponse("Validation failed."));

        var entity = new Phase { Name = dto.Name };
        await _uow.Phases.AddAsync(entity);
        await _uow.SaveChangesAsync();

        return StatusCode(201, ApiResponse<PhaseWithProcessesDto>.CreatedResponse(
            BuildPhaseDto(entity, [])));
    }

    /// <summary>List all phases with their associated processes and progress.</summary>
    /// <returns>List of phases with process details.</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<PhaseWithProcessesDto>>>> GetAll()
    {
        var phases = await _uow.Phases.GetAllAsync();
        var processes = await _uow.Processes.GetAllWithDetailsAsync();
        var processesByPhase = processes
            .GroupBy(p => p.PhaseId)
            .ToDictionary(g => g.Key, g => g.AsEnumerable());

        var result = phases.Select(phase =>
            BuildPhaseDto(
                phase,
                processesByPhase.TryGetValue(phase.Id, out var phaseProcesses)
                    ? phaseProcesses
                    : []));

        return Ok(ApiResponse<IEnumerable<PhaseWithProcessesDto>>.SuccessResponse(result));
    }

    /// <summary>Get a single phase by ID with its processes.</summary>
    /// <param name="id">Phase ID.</param>
    /// <returns>The phase with its associated processes.</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PhaseWithProcessesDto>>> GetById(int id)
    {
        var phase = await _uow.Phases.GetByIdAsync(id);
        if (phase is null)
            return NotFound(ApiResponse<PhaseWithProcessesDto>.NotFoundResponse());

        var processes = (await _uow.Processes.GetAllWithDetailsAsync())
            .Where(p => p.PhaseId == id);

        return Ok(ApiResponse<PhaseWithProcessesDto>.SuccessResponse(
            BuildPhaseDto(phase, processes)));
    }

    private static PhaseWithProcessesDto BuildPhaseDto(Phase phase, IEnumerable<Process> processes)
    {
        var processList = processes.ToList();
        var averageCompletion = processList.Count == 0
            ? 0m
            : processList.Average(p => p.ProcessStatus?.Percentage ?? 0m);

        return new PhaseWithProcessesDto(
            Id: phase.Id,
            Name: phase.Name,
            TotalProcesses: processList.Count,
            AverageCompletion: averageCompletion,
            Processes: processList.Select(p => p.ToDto()));
    }
}
