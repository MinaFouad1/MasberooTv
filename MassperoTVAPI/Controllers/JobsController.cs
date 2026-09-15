using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Helpers;
using MassperoTVAPI.Core.Interfaces;
using MassperoTVAPI.Core.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace MassperoTVAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,HR")]
public class JobsController : ControllerBase
{
    private readonly IUnitOfWork                  _uow;
    private readonly UserManager<ApplicationUser> _userManager;

    public JobsController(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
    {
        _uow         = uow;
        _userManager = userManager;
    }

    /// <summary>Retrieve all jobs with pagination and filtering.</summary>
    /// <param name="query">Pagination and filter options.</param>
    /// <returns>Paged list of jobs.</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<JobDto>>>> GetAll([FromQuery] GetJobsQueryDto query)
    {
        var pagedList = await _uow.Jobs.GetPagedAsync(query);
        var dtoItems = pagedList.Items.Select(j => j.ToDto()).ToList();
        var result = new PagedResult<JobDto>(
            dtoItems,
            pagedList.TotalCount,
            pagedList.Page,
            pagedList.PageSize,
            pagedList.TotalPages
        );
        return Ok(ApiResponse<PagedResult<JobDto>>.SuccessResponse(result));
    }

    /// <summary>Get recruitment progress for a specific job: applications, HR interviews, technical interviews, offers sent, accepted status "Hired", and hired counts.</summary>
    /// <param name="id">Job ID.</param>
    /// <returns>Job recruitment progress counts.</returns>
    [HttpGet("{id:int}/recruitment-progress")]
    [HttpGet("{id:int}/progress")]
    public async Task<ActionResult<ApiResponse<JobRecruitmentProgressDto>>> GetRecruitmentProgress(int? id)
    {
        var result = await _uow.Jobs.GetRecruitmentProgressAsync(id);
        if (result is null) return NotFound(ApiResponse<JobRecruitmentProgressDto>.NotFoundResponse());
        return Ok(ApiResponse<JobRecruitmentProgressDto>.SuccessResponse(result));
    }

    /// <summary>Get job statistics counts: total jobs, open positions, applications, offers sent.</summary>
    /// <returns>Job statistics.</returns>
    
    /// <summary>Get a single job by ID with its category name and all new fields.</summary>
    /// <param name="id">Job ID.</param>
    /// <returns>The job details.</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<JobDto>>> GetById(int id)
    {
        var entity = await _uow.Jobs.GetByIdWithCategoryAsync(id);
        if (entity is null) return NotFound(ApiResponse<JobDto>.NotFoundResponse());
        return Ok(ApiResponse<JobDto>.SuccessResponse(entity.ToDto()));
    }

    /// <summary>Create a new job. Accepts location, employment type, open positions, target hiring date, deadline date, job description, and isOpend.</summary>
    /// <param name="dto">Job data.</param>
    /// <returns>The created job.</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<JobDto>>> Create([FromBody] CreateJobDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<JobDto>.ErrorResponse("Validation failed."));

        if (!await _uow.Categories.ExistsAsync(dto.CategoryId))
            return BadRequest(ApiResponse<JobDto>.ErrorResponse($"Category {dto.CategoryId} not found."));

        if (dto.LocationId.HasValue && !await _uow.Locations.ExistsAsync(dto.LocationId.Value))
            return BadRequest(ApiResponse<JobDto>.ErrorResponse($"Location {dto.LocationId} not found."));

        if (!string.IsNullOrEmpty(dto.HiringManagerId))
        {
            var manager = await _userManager.FindByIdAsync(dto.HiringManagerId);
            if (manager is null)
                return BadRequest(ApiResponse<JobDto>.ErrorResponse($"User '{dto.HiringManagerId}' not found."));
        }

        var entity = new Job
        {
            Name             = dto.Name,
            Desc             = dto.JobDescription,
            CategoryId       = dto.CategoryId,
            OpenPositions    = dto.OpenPositions,
            EmploymentType   = dto.EmploymentType,
            TargetHiringDate = dto.TargetHiringDate,
            DeadLineDate     = dto.DeadLineDate,
            IsOpend          = dto.IsOpend,
            LocationId       = dto.LocationId,
            HiringManagerId  = dto.HiringManagerId,
            Responsibilities = dto.Responsibilities,
            RequiredSkills   = dto.RequiredSkills,
            ExperienceLevel  = dto.ExperienceLevel,
            Education        = dto.Education,
            Languages        = dto.Languages
        };
        await _uow.Jobs.AddAsync(entity);
        await _uow.SaveChangesAsync();

        var created = await _uow.Jobs.GetByIdWithCategoryAsync(entity.Id);
        return StatusCode(201, ApiResponse<JobDto>.CreatedResponse(created!.ToDto()));
    }

    /// <summary>Update an existing job including location, employment type, open positions, target hiring date, deadline date, job description, and isOpend.</summary>
    /// <param name="id">Job ID.</param>
    /// <param name="dto">Updated job data.</param>
    /// <returns>The updated job.</returns>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<JobDto>>> Update(int id, [FromBody] UpdateJobDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<JobDto>.ErrorResponse("Validation failed."));

        var entity = await _uow.Jobs.GetByIdAsync(id);
        if (entity is null) return NotFound(ApiResponse<JobDto>.NotFoundResponse());

        if (!await _uow.Categories.ExistsAsync(dto.CategoryId))
            return BadRequest(ApiResponse<JobDto>.ErrorResponse($"Category {dto.CategoryId} not found."));

        if (dto.LocationId.HasValue && !await _uow.Locations.ExistsAsync(dto.LocationId.Value))
            return BadRequest(ApiResponse<JobDto>.ErrorResponse($"Location {dto.LocationId} not found."));

        if (!string.IsNullOrEmpty(dto.HiringManagerId))
        {
            var manager = await _userManager.FindByIdAsync(dto.HiringManagerId);
            if (manager is null)
                return BadRequest(ApiResponse<JobDto>.ErrorResponse($"User '{dto.HiringManagerId}' not found."));
        }

        entity.Name             = dto.Name;
        entity.Desc             = dto.JobDescription;
        entity.CategoryId       = dto.CategoryId;
        entity.OpenPositions    = dto.OpenPositions;
        entity.EmploymentType   = dto.EmploymentType;
        entity.TargetHiringDate = dto.TargetHiringDate;
        entity.DeadLineDate     = dto.DeadLineDate;
        entity.IsOpend          = dto.IsOpend;
        entity.LocationId       = dto.LocationId;
        entity.HiringManagerId  = dto.HiringManagerId;
        entity.Responsibilities = dto.Responsibilities;
        entity.RequiredSkills   = dto.RequiredSkills;
        entity.ExperienceLevel  = dto.ExperienceLevel;
        entity.Education        = dto.Education;
        entity.Languages        = dto.Languages;

        _uow.Jobs.Update(entity);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Jobs.GetByIdWithCategoryAsync(entity.Id);
        return Ok(ApiResponse<JobDto>.SuccessResponse(updated!.ToDto()));
    }

    /// <summary>Delete a job. Admin only.</summary>
    /// <param name="id">Job ID.</param>
    /// <returns>Success status.</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var entity = await _uow.Jobs.GetByIdAsync(id);
            if (entity is null) return NotFound(ApiResponse<object>.NotFoundResponse());
            _uow.Jobs.Delete(entity);
            await _uow.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse($"Can not delete this one {ex.Message}"));
        }
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Deleted succesffuly"));
    }
}
