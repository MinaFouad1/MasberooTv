using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Helpers;
using MassperoTVAPI.Core.Interfaces;
using MassperoTVAPI.Core.Mappers;
using MassperoTVAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MassperoTVAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "HR,Admin")]
public class CandidatesController : ControllerBase
{
    private const string CvConfigurationKey = "CvFile";
    private const string InitialStatusName = "Under Vetting";
    private const string InitialSecurityClearanceName = "In Check";

    private readonly IUnitOfWork _uow;
    private readonly IFileUploadService _fileUploadService;

    public CandidatesController(IUnitOfWork uow, IFileUploadService fileUploadService)
    {
        _uow = uow;
        _fileUploadService = fileUploadService;
    }

    /// <summary>
    /// List candidates (applications) with optional filters and pagination.
    /// Filters: candidateName, jobId, categoryId, statusId, dateFrom (HiringDate), dateTo (HiringDate).
    /// </summary>
    /// <param name="query">Search and pagination parameters.</param>
    /// <returns>Paged list of candidates with full details including HR/Technical scores.</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<CandidateDto>>>> GetAll(
        [FromQuery] GetApplicationsQueryDto query)
    {
        var safePage     = Math.Max(1, query.Page);
        var safePageSize = Math.Clamp(query.PageSize, 1, 100);

        var (items, totalCount) = await _uow.Candidates.GetPagedAsync(
            query.CandidateName,
            query.JobId,
            query.CategoryId,
            query.StatusId,
            query.DateFrom,
            query.DateTo,
            safePage,
            safePageSize);

        var cvBaseUrl  = await GetCvBaseUrlAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)safePageSize);

        var result = new PagedResult<CandidateDto>(
            items.Select(c => c.ToDto(cvBaseUrl)),
            totalCount,
            safePage,
            safePageSize,
            totalPages);

        return Ok(ApiResponse<PagedResult<CandidateDto>>.SuccessResponse(result));
    }

    /// <summary>Get a single candidate by ID with full details.</summary>
    /// <param name="id">Candidate ID.</param>
    /// <returns>The candidate details including job, status, and security clearance.</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<CandidateDto>>> GetById(int id)
    {
        var entity = await _uow.Candidates.GetByIdWithDetailsAsync(id);
        if (entity is null) return NotFound(ApiResponse<CandidateDto>.NotFoundResponse());
        return Ok(ApiResponse<CandidateDto>.SuccessResponse(entity.ToDto(await GetCvBaseUrlAsync())));
    }

    /// <summary>Create a new candidate with initial status and security clearance. Optionally upload a CV file.</summary>
    /// <param name="dto">Candidate data including name, job ID, and optional CV file.</param>
    /// <returns>The created candidate with default candidate statuses assigned.</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CandidateDto>>> Create([FromForm] CreateCandidateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<CandidateDto>.ErrorResponse("Validation failed."));

        if (!await _uow.Jobs.ExistsAsync(dto.JobId))
            return BadRequest(ApiResponse<CandidateDto>.ErrorResponse($"Job {dto.JobId} not found."));

        var initialStatus = await GetStatusByNameAsync(InitialStatusName);
        if (initialStatus is null)
            return BadRequest(ApiResponse<CandidateDto>.ErrorResponse($"Status '{InitialStatusName}' not found."));

        var initialSecurityClearance = await GetSecurityClearanceByNameAsync(InitialSecurityClearanceName);
        if (initialSecurityClearance is null)
            return BadRequest(ApiResponse<CandidateDto>.ErrorResponse($"SecurityClearance '{InitialSecurityClearanceName}' not found."));

        string? cvFileName = null;
        if (dto.CvFile is not null)
        {
            var upload = await UploadCandidateCvAsync(dto.CvFile);
            if (upload.Error is not null)
                return BadRequest(ApiResponse<CandidateDto>.ErrorResponse(upload.Error));

            cvFileName = upload.FileName;
        }

        var entity = new Candidate
        {
            Name                = dto.Name,
            CvFile              = cvFileName,
            JobId               = dto.JobId,
            StatusId            = initialStatus.Id,
            SecurityClearanceId = initialSecurityClearance.Id,
        };

        await _uow.Candidates.AddAsync(entity);
        await _uow.SaveChangesAsync();

        var created = await _uow.Candidates.GetByIdWithDetailsAsync(entity.Id);
        return StatusCode(201, ApiResponse<CandidateDto>.CreatedResponse(created!.ToDto(await GetCvBaseUrlAsync())));
    }

    /// <summary>Update a candidate's basic info (name, job, reasons, CV).</summary>
    /// <param name="id">Candidate ID.</param>
    /// <param name="dto">Updated candidate data.</param>
    /// <returns>The updated candidate.</returns>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<CandidateDto>>> Update(int id, [FromForm] UpdateCandidateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<CandidateDto>.ErrorResponse("Validation failed."));

        var entity = await _uow.Candidates.GetByIdAsync(id);
        if (entity is null) return NotFound(ApiResponse<CandidateDto>.NotFoundResponse());

        if (!await _uow.Jobs.ExistsAsync(dto.JobId))
            return BadRequest(ApiResponse<CandidateDto>.ErrorResponse($"Job {dto.JobId} not found."));

        if (dto.CvFile is not null)
        {
            var upload = await UploadCandidateCvAsync(dto.CvFile);
            if (upload.Error is not null)
                return BadRequest(ApiResponse<CandidateDto>.ErrorResponse(upload.Error));

            entity.CvFile = upload.FileName;
        }

        entity.Name                = dto.Name;
        entity.ReasonOfAccept      = dto.ReasonOfAccept;
        entity.ReasonOfReject      = dto.ReasonOfReject;
        entity.JobId               = dto.JobId;

        _uow.Candidates.Update(entity);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Candidates.GetByIdWithDetailsAsync(entity.Id);
        return Ok(ApiResponse<CandidateDto>.SuccessResponse(updated!.ToDto(await GetCvBaseUrlAsync())));
    }

    /// <summary>Update only the application status for a candidate (e.g. Under Vetting, Approved, Rejected).</summary>
    /// <param name="id">Candidate ID.</param>
    /// <param name="dto">New status ID.</param>
    /// <returns>The updated candidate.</returns>
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<ApiResponse<CandidateDto>>> PatchStatus(
        int id, [FromBody] PatchCandidateStatusDto dto)
    {
        var entity = await _uow.Candidates.GetByIdAsync(id);
        if (entity is null) return NotFound(ApiResponse<CandidateDto>.NotFoundResponse());

        if (!await _uow.Statuses.ExistsAsync(dto.StatusId))
            return BadRequest(ApiResponse<CandidateDto>.ErrorResponse($"Status {dto.StatusId} not found."));

        entity.StatusId = dto.StatusId;
        _uow.Candidates.Update(entity);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Candidates.GetByIdWithDetailsAsync(entity.Id);
        return Ok(ApiResponse<CandidateDto>.SuccessResponse(updated!.ToDto(await GetCvBaseUrlAsync()), "Candidate status updated."));
    }

    /// <summary>Update only the security clearance status for a candidate (e.g. In Check, Cleared, Denied).</summary>
    /// <param name="id">Candidate ID.</param>
    /// <param name="dto">New security clearance ID.</param>
    /// <returns>The updated candidate.</returns>
    [HttpPatch("{id:int}/security-clearance")]
    public async Task<ActionResult<ApiResponse<CandidateDto>>> PatchSecurityClearance(
        int id, [FromBody] PatchCandidateSecurityDto dto)
    {
        var entity = await _uow.Candidates.GetByIdAsync(id);
        if (entity is null) return NotFound(ApiResponse<CandidateDto>.NotFoundResponse());

        if (!await _uow.SecurityClearances.ExistsAsync(dto.SecurityClearanceId))
            return BadRequest(ApiResponse<CandidateDto>.ErrorResponse($"SecurityClearance {dto.SecurityClearanceId} not found."));

        entity.SecurityClearanceId = dto.SecurityClearanceId;
        _uow.Candidates.Update(entity);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Candidates.GetByIdWithDetailsAsync(entity.Id);
        return Ok(ApiResponse<CandidateDto>.SuccessResponse(updated!.ToDto(await GetCvBaseUrlAsync()), "Security clearance updated."));
    }

    /// <summary>Delete a candidate. Admin only.</summary>
    /// <param name="id">Candidate ID.</param>
    /// <returns>Success status.</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var entity = await _uow.Candidates.GetByIdAsync(id);
            if (entity is null) return NotFound(ApiResponse<object>.NotFoundResponse());
            _uow.Candidates.Delete(entity);
            await _uow.SaveChangesAsync();
        }
        catch(Exception ex)
        
        {
            return BadRequest(ApiResponse<object>.ErrorResponse($"Can not delete this one {ex.Message }"));
        }
    
        return Ok(ApiResponse<bool>.SuccessResponse(true,"Deleted succesffuly"));
    }

    private async Task<string?> GetCvBaseUrlAsync()
        => (await _uow.Configurations.GetByKeyAsync(CvConfigurationKey))?.Value;

    private async Task<Status?> GetStatusByNameAsync(string name)
        => (await _uow.Statuses.GetAllAsync())
            .FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

    private async Task<SecurityClearanceStatus?> GetSecurityClearanceByNameAsync(string name)
        => (await _uow.SecurityClearances.GetAllAsync())
            .FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

    private async Task<(string? FileName, string? Error)> UploadCandidateCvAsync(IFormFile file)
    {
        try
        {
            var upload = await _fileUploadService.UploadAsync(file, CvConfigurationKey);
            return (upload.FileName, null);
        }
        catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException)
        {
            return (null, ex.Message);
        }
    }
}
