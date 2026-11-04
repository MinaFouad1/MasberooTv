using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Helpers;
using MassperoTVAPI.Core.Interfaces;
using MassperoTVAPI.Core.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MassperoTVAPI.Modules.Hr.Domain.Entities.Identity;
using MassperoTVAPI.Modules.Hr.Domain.Enums;
using MassperoTVAPI.Shared.Persistence;

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
    private readonly UserManager<ApplicationUser> _userManager;

    public LookupsController(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
    {
        _uow = uow;
        _userManager = userManager;
    }

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

    // ════════════════════════════════════════════════════════════
    // LANGUAGES
    // ════════════════════════════════════════════════════════════

    /// <summary>List all available languages for candidate selection.</summary>
    [HttpGet("languages")]
    public async Task<ActionResult<ApiResponse<IEnumerable<LanguageDto>>>> GetLanguages()
    {
        var list = await _uow.Languages.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<LanguageDto>>.SuccessResponse(list.Select(l => l.ToDto())));
    }

    /// <summary>Create a new language.</summary>
    [HttpPost("languages")]
    public async Task<ActionResult<ApiResponse<LanguageDto>>> CreateLanguage([FromBody] CreateLanguageDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(ApiResponse<LanguageDto>.ErrorResponse("Language name is required."));

        var name = dto.Name.Trim();
        var existing = (await _uow.Languages.GetAllAsync()).FirstOrDefault(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
            return Ok(ApiResponse<LanguageDto>.SuccessResponse(existing.ToDto(), "Language already exists."));

        var language = new Language { Name = name };
        await _uow.Languages.AddAsync(language);
        await _uow.SaveChangesAsync();

        return StatusCode(201, ApiResponse<LanguageDto>.CreatedResponse(language.ToDto()));
    }

    // ════════════════════════════════════════════════════════════
    // SKILLS
    // ════════════════════════════════════════════════════════════

    /// <summary>List all available skills for candidate selection.</summary>
    [HttpGet("skills")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SkillDto>>>> GetSkills()
    {
        var list = await _uow.Skills.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<SkillDto>>.SuccessResponse(list.Select(s => s.ToDto())));
    }

    /// <summary>Create a new skill.</summary>
    [HttpPost("skills")]
    public async Task<ActionResult<ApiResponse<SkillDto>>> CreateSkill([FromBody] CreateSkillDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(ApiResponse<SkillDto>.ErrorResponse("Skill name is required."));

        var name = dto.Name.Trim();
        var existing = (await _uow.Skills.GetAllAsync()).FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
            return Ok(ApiResponse<SkillDto>.SuccessResponse(existing.ToDto(), "Skill already exists."));

        var skill = new Skill { Name = name };
        await _uow.Skills.AddAsync(skill);
        await _uow.SaveChangesAsync();

        return StatusCode(201, ApiResponse<SkillDto>.CreatedResponse(skill.ToDto()));
    }

    // ════════════════════════════════════════════════════════════
    // LOCATIONS
    // ════════════════════════════════════════════════════════════

    /// <summary>List all locations with ID and Name.</summary>
    [HttpGet("locations")]
    public async Task<ActionResult<ApiResponse<IEnumerable<LocationDto>>>> GetLocations()
    {
        var list = await _uow.Locations.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<LocationDto>>.SuccessResponse(list.Select(l => l.ToDto())));
    }

    // ════════════════════════════════════════════════════════════
    // HIRING MANAGERS
    // ════════════════════════════════════════════════════════════

    /// <summary>List all hiring managers (users with the HR role).</summary>
    [HttpGet("hiringmanagers")]
    public async Task<ActionResult<ApiResponse<IEnumerable<HiringManagerDto>>>> GetHiringManagers()
    {
        var hrUsers = await _userManager.GetUsersInRoleAsync("HR");
        var list = hrUsers
            .OrderBy(u => u.UserName)
            .Select(u => new HiringManagerDto(u.Id, u.UserName ?? string.Empty, u.Email));

        return Ok(ApiResponse<IEnumerable<HiringManagerDto>>.SuccessResponse(list));
    }
}

