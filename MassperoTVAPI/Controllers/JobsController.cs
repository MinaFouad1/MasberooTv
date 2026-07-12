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

    /// <summary>List all jobs with their category names. Optionally filter by title, category, creation date, or location.</summary>
    /// <param name="search">Optional search keyword for job title.</param>
    /// <param name="categoryId">Optional category ID filter.</param>
    /// <param name="date">Optional date filter — returns jobs created on or after this date.</param>
    /// <param name="locationId">Optional location ID filter.</param>
    /// <returns>List of jobs with applicant and interview counts.</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<JobDto>>>> GetAll(
        [FromQuery] string?   search          = null,
        [FromQuery] int?      categoryId      = null,
        [FromQuery] DateTime? date            = null,
        [FromQuery] int?      locationId      = null,
        [FromQuery] string?   hiringManagerId = null)
    {
        var list = await _uow.Jobs.GetAllWithCategoryAsync(search, categoryId, date, locationId, hiringManagerId);
        return Ok(ApiResponse<IEnumerable<JobDto>>.SuccessResponse(
            list.Select(j => j.ToDto())));
    }

    /// <summary>Get job statistics counts: total jobs, open positions, applications, offers sent.</summary>
    /// <returns>Job statistics.</returns>
    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<JobStatisticsDto>>> GetStatistics()
    {
        var jobs = (await _uow.Jobs.GetAllWithCandidatesAsync()).ToList();

        var totalJobs      = jobs.Count;
        var openPositions  = jobs.Sum(j => j.OpenPositions);
        var applications   = jobs.Sum(j => j.Candidates.Count);

        // "Offers Sent" = candidates whose status name is "Under Vetting"
        var offersSent = jobs.Sum(j =>
            j.Candidates.Count(c =>
                string.Equals(c.Status?.Name, "Offered", StringComparison.OrdinalIgnoreCase)));

        var offersAccepted = jobs.Sum(j =>
            j.Candidates.Count(c =>
                string.Equals(c.Status?.Name, "Hired", StringComparison.OrdinalIgnoreCase)));

        return Ok(ApiResponse<JobStatisticsDto>.SuccessResponse(new JobStatisticsDto(
            TotalJobs:     totalJobs,
            OpenPositions: openPositions,
            Applications:  applications,
            OffersSent:    offersSent,
            OffersAccepted: offersAccepted
        )));
    }

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

    /// <summary>Create a new job. Accepts location, employment type, open positions, and target hiring date.</summary>
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
            Desc             = dto.Desc,
            CategoryId       = dto.CategoryId,
            OpenPositions    = dto.OpenPositions,
            EmploymentType   = dto.EmploymentType,
            TargetHiringDate = dto.TargetHiringDate,
            LocationId       = dto.LocationId,
            HiringManagerId  = dto.HiringManagerId
        };
        await _uow.Jobs.AddAsync(entity);
        await _uow.SaveChangesAsync();

        var created = await _uow.Jobs.GetByIdWithCategoryAsync(entity.Id);
        return StatusCode(201, ApiResponse<JobDto>.CreatedResponse(created!.ToDto()));
    }

    /// <summary>Update an existing job including location, employment type, open positions, and target hiring date.</summary>
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
        entity.Desc             = dto.Desc;
        entity.CategoryId       = dto.CategoryId;
        entity.OpenPositions    = dto.OpenPositions;
        entity.EmploymentType   = dto.EmploymentType;
        entity.TargetHiringDate = dto.TargetHiringDate;
        entity.LocationId       = dto.LocationId;
        entity.HiringManagerId  = dto.HiringManagerId;

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
