using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Enums;
using MassperoTVAPI.Core.Helpers;
using MassperoTVAPI.Core.Interfaces;
using MassperoTVAPI.Core.Mappers;
using MassperoTVAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MassperoTVAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "HR,Admin")]
public class CandidatesController : ControllerBase
{
    private const string CvConfigurationKey           = "CvFile";
    private const string ProfileConfigurationKey       = "ProfileImage";
    private const string InitialStatusName             = "Under Vetting";
    private const string InitialSecurityClearanceName  = "In Check";
    // Pipeline status names
    private const string HiredStatusName               = "Hired";
    private const string RejectedStatusName            = "Rejected";
    private const string SignContractStatusName        = "SignContract";
    private const string PipelineCompleted             = "Completed";
    private const string PipelineInProgress            = "InProgress";
    private const string PipelinePending               = "Pending";

    private readonly IUnitOfWork _uow;
    private readonly IFileUploadService _fileUploadService;
    private readonly ICvParserService _cvParserService;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    public CandidatesController(
        IUnitOfWork uow,
        IFileUploadService fileUploadService,
        ICvParserService cvParserService,
        IConfiguration config,
        IWebHostEnvironment env)
    {
        _uow = uow;
        _fileUploadService = fileUploadService;
        _cvParserService = cvParserService;
        _config = config;
        _env = env;
    }

 
 
    [HttpGet("applicants")]
    public async Task<ActionResult<ApiResponse<PagedResult<ApplicantDto>>>> GetApplicants(
        [FromQuery] GetApplicantsQueryDto query)
    {
        var (_, profileBaseUrl) = await GetBaseUrlsAsync();
        var (items, totalCount) = await _uow.Candidates.GetApplicantsAsync(query);

        var safePage     = Math.Max(1, query.PageNumber);
        var safePageSize = Math.Clamp(query.PageSize, 1, 100);

        var dtos = items.Select(c => c.ToApplicantDto(profileBaseUrl)).ToList();

        var result = new PagedResult<ApplicantDto>(
            dtos,
            totalCount,
            safePage,
            safePageSize,
            (int)Math.Ceiling(totalCount / (double)safePageSize));

        return Ok(ApiResponse<PagedResult<ApplicantDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Get a single candidate by ID with full details.
    /// Returns: profile image, CV file, job info, all interview scores (HR / Technical / Overall),
    /// hiring probability, ranking position among same-job candidates, and current status.
    /// </summary>
    /// <param name="id">Candidate ID.</param>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<CandidateDetailDto>>> GetById(int id)
    {
        var entity = await _uow.Candidates.GetByIdWithDetailsAsync(id);
        if (entity is null)
            return NotFound(ApiResponse<CandidateDetailDto>.NotFoundResponse());

        // Compute ranking among all candidates for the same job
        var rankInfo = await _uow.Candidates.GetRankInJobAsync(entity.Id, entity.JobId);

        var (cvBaseUrl, profileBaseUrl) = await GetBaseUrlsAsync();
        return Ok(ApiResponse<CandidateDetailDto>.SuccessResponse(
            entity.ToDetailDto(rankInfo.Rank, rankInfo.TotalCandidates, cvBaseUrl, profileBaseUrl)));
    }

    /// <summary>Get all interviews recorded for a candidate.</summary>
    /// <param name="id">Candidate ID.</param>
    /// <returns>Interview type, grade, creation time, and evaluator details when available.</returns>
    [Authorize(Roles = "HR,Admin")]
    [HttpGet("{id:int}/interviews")]
    public async Task<ActionResult<ApiResponse<IEnumerable<InterviewDto>>>> GetInterviews(int id)
    {
        if (!await _uow.Candidates.ExistsAsync(id))
            return NotFound(ApiResponse<IEnumerable<InterviewDto>>.NotFoundResponse("Candidate not found."));

        var interviews = await _uow.Interviews.GetByCandidateAsync(id);
        return Ok(ApiResponse<IEnumerable<InterviewDto>>.SuccessResponse(
            interviews.OrderByDescending(i => i.CreatedAt).Select(i => i.ToDto())));
    }

    /// <summary>
    /// Create a new candidate with initial status and security clearance.
    /// Optionally upload a CV file and/or a profile image.
    /// </summary>
    /// <param name="dto">Candidate data including name, job ID, optional CV file and optional profile image.</param>
    /// <returns>The created candidate.</returns>
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

        // ── CV upload (optional) ──────────────────────────────────────────────
        string? cvFileName = null;
        if (dto.CvFile is not null)
        {
            var upload = await UploadFileAsync(dto.CvFile, CvConfigurationKey);
            if (upload.Error is not null)
                return BadRequest(ApiResponse<CandidateDto>.ErrorResponse(upload.Error));
            cvFileName = upload.FileName;
        }

        // ── Profile image upload (optional) ──────────────────────────────────
        string? profileFileName = null;
        if (dto.ProfileImage is not null)
        {
            var upload = await UploadFileAsync(dto.ProfileImage, ProfileConfigurationKey);
            if (upload.Error is not null)
                return BadRequest(ApiResponse<CandidateDto>.ErrorResponse(upload.Error));
            profileFileName = upload.FileName;
        }

        var entity = new Candidate
        {
            Name                = dto.Name,
            CvFile              = cvFileName,
            Profile             = profileFileName,
            JobId               = dto.JobId,
            StatusId            = initialStatus.Id,
            SecurityClearanceId = initialSecurityClearance.Id,
            CurrentEmployer     = dto.CurrentEmployer,
            CurrentPosition     = dto.CurrentPosition,
            YearsOfExperience   = dto.YearsOfExperience,
            ExpectedSalary      = dto.ExpectedSalary,
            NoticePeriod        = dto.NoticePeriod,
            Availability        = dto.Availability,
            Summary             = dto.Summary,
            CurrentSalary       = dto.CurrentSalary,

            // Personal info
            Gender               = dto.Gender,
            NationalId           = dto.NationalId,
            Address              = dto.Address,
            Mobile               = dto.Mobile,
            AlternateMobile      = dto.AlternateMobile,
            Email                = dto.Email,
            MaritalStatus        = dto.MaritalStatus,
            DateOfBeginning      = dto.DateOfBeginning,
            Nationality          = dto.Nationality,
            PreferredJobLocation = dto.PreferredJobLocation,
            PreferredShift       = dto.PreferredShift,
            City                 = dto.City,
            Country              = dto.Country,
        };

        await _uow.Candidates.AddAsync(entity);
        await _uow.SaveChangesAsync();
        await _uow.Candidates.SyncProfileCollectionsAsync(
            entity,
            dto.Skills,
            dto.Languages,
            dto.Certifications,
            dto.Educations);
        await _uow.SaveChangesAsync();

        var created = await _uow.Candidates.GetByIdWithDetailsAsync(entity.Id);
        var (cvBaseUrl, profileBaseUrl) = await GetBaseUrlsAsync();
        return StatusCode(201, ApiResponse<CandidateDto>.CreatedResponse(created!.ToDto(cvBaseUrl, profileBaseUrl)));
    }

    /// <summary>Update a candidate's basic info (name, job, reasons, CV, profile image).</summary>
    /// <param name="id">Candidate ID.</param>
    /// <param name="dto">Updated candidate data.</param>
    /// <returns>The updated candidate.</returns>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<CandidateDto>>> Update(int id, [FromForm] UpdateCandidateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<CandidateDto>.ErrorResponse("Validation failed."));

        var entity = await _uow.Candidates.GetByIdWithDetailsAsync(id);
        if (entity is null) return NotFound(ApiResponse<CandidateDto>.NotFoundResponse());

        if (!await _uow.Jobs.ExistsAsync(dto.JobId))
            return BadRequest(ApiResponse<CandidateDto>.ErrorResponse($"Job {dto.JobId} not found."));

        // ── CV upload (optional — only replaces if a new file is sent) ────────
        if (dto.CvFile is not null)
        {
            var upload = await UploadFileAsync(dto.CvFile, CvConfigurationKey);
            if (upload.Error is not null)
                return BadRequest(ApiResponse<CandidateDto>.ErrorResponse(upload.Error));
            entity.CvFile = upload.FileName;
        }

        // ── Profile image upload (optional — only replaces if a new file is sent)
        if (dto.ProfileImage is not null)
        {
            var upload = await UploadFileAsync(dto.ProfileImage, ProfileConfigurationKey);
            if (upload.Error is not null)
                return BadRequest(ApiResponse<CandidateDto>.ErrorResponse(upload.Error));
            entity.Profile = upload.FileName;
        }

        entity.Name           = dto.Name;
        entity.ReasonOfAccept = dto.ReasonOfAccept;
        entity.ReasonOfReject = dto.ReasonOfReject;
        entity.JobId          = dto.JobId;
        entity.CurrentEmployer   = dto.CurrentEmployer;
        entity.CurrentPosition   = dto.CurrentPosition;
        entity.YearsOfExperience = dto.YearsOfExperience;
        entity.ExpectedSalary    = dto.ExpectedSalary;
        entity.NoticePeriod      = dto.NoticePeriod;
        entity.Availability      = dto.Availability;
        entity.Summary           = dto.Summary;
        entity.CurrentSalary     = dto.CurrentSalary;

        // Personal info
        entity.Gender               = dto.Gender;
        entity.NationalId           = dto.NationalId;
        entity.Address              = dto.Address;
        entity.Mobile               = dto.Mobile;
        entity.AlternateMobile      = dto.AlternateMobile;
        entity.Email                = dto.Email;
        entity.MaritalStatus        = dto.MaritalStatus;
        entity.DateOfBeginning      = dto.DateOfBeginning;
        entity.Nationality          = dto.Nationality;
        entity.PreferredJobLocation = dto.PreferredJobLocation;
        entity.PreferredShift       = dto.PreferredShift;
        entity.City                 = dto.City;
        entity.Country              = dto.Country;

        await _uow.Candidates.SyncProfileCollectionsAsync(
            entity,
            dto.Skills,
            dto.Languages,
            dto.Certifications,
            dto.Educations);

        _uow.Candidates.Update(entity);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Candidates.GetByIdWithDetailsAsync(entity.Id);
        var (cvBaseUrl, profileBaseUrl) = await GetBaseUrlsAsync();
        return Ok(ApiResponse<CandidateDto>.SuccessResponse(updated!.ToDto(cvBaseUrl, profileBaseUrl)));
    }

    /// <summary>Update only the application status for a candidate (e.g. Under Vetting, Approved, Rejected, Hired, SignContract).</summary>
    /// <param name="id">Candidate ID.</param>
    /// <param name="dto">New status ID.</param>
    /// <returns>The updated candidate.</returns>
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<ApiResponse<CandidateDto>>> PatchStatus(
        int id, [FromBody] PatchCandidateStatusDto dto)
    {
        var entity = await _uow.Candidates.GetByIdAsync(id);
        if (entity is null) return NotFound(ApiResponse<CandidateDto>.NotFoundResponse());

        var status = await _uow.Statuses.GetByIdAsync(dto.StatusId);
        if (status is null)
            return BadRequest(ApiResponse<CandidateDto>.ErrorResponse($"Status {dto.StatusId} not found."));

        entity.StatusId = dto.StatusId;
        entity.Status = status;

        if (string.Equals(status.Name, HiredStatusName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status.Name, SignContractStatusName, StringComparison.OrdinalIgnoreCase))
        {
            entity.Accepted = true;
            entity.HiringDate ??= DateTime.UtcNow;
        }
        else if (string.Equals(status.Name, RejectedStatusName, StringComparison.OrdinalIgnoreCase))
        {
            entity.Accepted = false;
            entity.HiringDate = null;
        }

        _uow.Candidates.Update(entity);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Candidates.GetByIdWithDetailsAsync(entity.Id);
        var (cvBaseUrl, profileBaseUrl) = await GetBaseUrlsAsync();
        return Ok(ApiResponse<CandidateDto>.SuccessResponse(updated!.ToDto(cvBaseUrl, profileBaseUrl), "Candidate status updated."));
    }

    /// <summary>Approve a candidate: sets status to 'Hired', sets HiringDate, marks Accepted = true, and optional ReasonOfAccept.</summary>
    /// <param name="id">Candidate ID.</param>
    /// <param name="dto">Optional approval payload with HiringDate and ReasonOfAccept.</param>
    /// <returns>The updated candidate.</returns>
    [HttpPatch("{id:int}/approve")]
    public async Task<ActionResult<ApiResponse<CandidateDto>>> ApproveCandidate(
        int id,
        [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] ApproveCandidateDto? dto = null)
    {
        var candidate = await _uow.Candidates.GetByIdAsync(id);
        if (candidate is null) return NotFound(ApiResponse<CandidateDto>.NotFoundResponse());

        var hiredStatus = await GetOrCreateStatusByNameAsync(HiredStatusName);

        candidate.StatusId = hiredStatus.Id;
        candidate.Status = hiredStatus;
        candidate.Accepted = true;
        candidate.HiringDate = dto?.HiringDate ?? DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(dto?.ReasonOfAccept))
            candidate.ReasonOfAccept = dto.ReasonOfAccept.Trim();

        _uow.Candidates.Update(candidate);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Candidates.GetByIdWithDetailsAsync(candidate.Id);
        var (cvBaseUrl, profileBaseUrl) = await GetBaseUrlsAsync();
        return Ok(ApiResponse<CandidateDto>.SuccessResponse(
            updated!.ToDto(cvBaseUrl, profileBaseUrl), "Candidate approved and marked as hired."));
    }

    /// <summary>Reject a candidate: sets status to 'Rejected', marks Accepted = false, clears HiringDate, and optional ReasonOfReject.</summary>
    /// <param name="id">Candidate ID.</param>
    /// <param name="dto">Optional rejection payload with ReasonOfReject.</param>
    /// <returns>The updated candidate.</returns>
    [HttpPatch("{id:int}/reject")]
    public async Task<ActionResult<ApiResponse<CandidateDto>>> RejectCandidate(
        int id,
        [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] RejectCandidateDto? dto = null)
    {
        var candidate = await _uow.Candidates.GetByIdAsync(id);
        if (candidate is null) return NotFound(ApiResponse<CandidateDto>.NotFoundResponse());

        var rejectedStatus = await GetOrCreateStatusByNameAsync(RejectedStatusName);

        candidate.StatusId = rejectedStatus.Id;
        candidate.Status = rejectedStatus;
        candidate.Accepted = false;
        candidate.HiringDate = null;

        if (!string.IsNullOrWhiteSpace(dto?.ReasonOfReject))
            candidate.ReasonOfReject = dto.ReasonOfReject.Trim();

        _uow.Candidates.Update(candidate);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Candidates.GetByIdWithDetailsAsync(candidate.Id);
        var (cvBaseUrl, profileBaseUrl) = await GetBaseUrlsAsync();
        return Ok(ApiResponse<CandidateDto>.SuccessResponse(
            updated!.ToDto(cvBaseUrl, profileBaseUrl), "Candidate marked as rejected."));
    }

    /// <summary>Sign contract for a candidate: sets status to 'SignContract', marks candidate as hired, sets HiringDate, and marks Accepted = true.</summary>
    /// <param name="id">Candidate ID.</param>
    /// <param name="dto">Optional sign-contract payload with HiringDate and Notes.</param>
    /// <returns>The updated candidate.</returns>
    [HttpPatch("{id:int}/sign-contract")]
    public async Task<ActionResult<ApiResponse<CandidateDto>>> SignContract(
        int id,
        [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] SignContractCandidateDto? dto = null)
    {
        var candidate = await _uow.Candidates.GetByIdAsync(id);
        if (candidate is null) return NotFound(ApiResponse<CandidateDto>.NotFoundResponse());

        var signContractStatus = await GetOrCreateStatusByNameAsync(SignContractStatusName);

        candidate.StatusId = signContractStatus.Id;
        candidate.Status = signContractStatus;
        candidate.Accepted = true;
        candidate.HiringDate = dto?.HiringDate ?? DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(dto?.Notes) && string.IsNullOrWhiteSpace(candidate.ReasonOfAccept))
            candidate.ReasonOfAccept = dto.Notes.Trim();

        _uow.Candidates.Update(candidate);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Candidates.GetByIdWithDetailsAsync(candidate.Id);
        var (cvBaseUrl, profileBaseUrl) = await GetBaseUrlsAsync();
        return Ok(ApiResponse<CandidateDto>.SuccessResponse(
            updated!.ToDto(cvBaseUrl, profileBaseUrl), "Contract signed successfully. Candidate is marked as hired."));
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
        var (cvBaseUrl, profileBaseUrl) = await GetBaseUrlsAsync();
        return Ok(ApiResponse<CandidateDto>.SuccessResponse(updated!.ToDto(cvBaseUrl, profileBaseUrl), "Security clearance updated."));
    }

    /// <summary>Update interview grade for a candidate (e.g. HR or Technical).</summary>
    /// <param name="id">Candidate ID.</param>
    /// <param name="dto">Interview type, grade, and comments.</param>
    /// <returns>The updated candidate details.</returns>
    [Authorize]
    [HttpPatch("{id:int}/grade")]
    public async Task<ActionResult<ApiResponse<CandidateDetailDto>>> PatchGrade(
        int id, [FromBody] PatchCandidateInterviewGradeDto dto)
    {
        var evaluatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(evaluatorId))
            return Unauthorized(ApiResponse<CandidateDetailDto>.UnauthorizedResponse("Authenticated user was not found."));

        var candidate = await _uow.Candidates.GetByIdWithDetailsAsync(id);
        if (candidate is null) return NotFound(ApiResponse<CandidateDetailDto>.NotFoundResponse());

        var typeKeyword = dto.InterviewType.Trim();
        var interview = candidate.Interviews.FirstOrDefault(i =>
            i.Type?.Name != null &&
            i.Type.Name.Contains(typeKeyword, StringComparison.OrdinalIgnoreCase));

        string resolvedTypeName = interview?.Type?.Name ?? string.Empty;

        if (interview is  null)
            return BadRequest(ApiResponse<CandidateDto>.ErrorResponse($"Interview  not Sceduled yet ,,schedule that First."));

            interview.Grade = dto.Grade;
            interview.CreatedAt = DateTime.UtcNow;
            interview.EvaluatorId = evaluatorId;
            if (dto.Comments is not null)
                interview.Comments = dto.Comments;
            _uow.Interviews.Update(interview);
        
    

        // Update candidate status based on interview type (e.g., HR Interview / Technical Interview)
        var statuses = (await _uow.Statuses.GetAllAsync()).ToList();
        Status? matchingStatus = null;

        if (typeKeyword.Contains("HR", StringComparison.OrdinalIgnoreCase) || resolvedTypeName.Contains("HR", StringComparison.OrdinalIgnoreCase))
        {
            matchingStatus = statuses.FirstOrDefault(s =>
                s.Name.Contains("HR", StringComparison.OrdinalIgnoreCase) &&
                (s.Name.Contains("interv", StringComparison.OrdinalIgnoreCase) || s.Name.Contains("inerv", StringComparison.OrdinalIgnoreCase)))
                ?? statuses.FirstOrDefault(s => s.Name.Contains("HR", StringComparison.OrdinalIgnoreCase));
        }
        else if (typeKeyword.Contains("Tech", StringComparison.OrdinalIgnoreCase) || resolvedTypeName.Contains("Tech", StringComparison.OrdinalIgnoreCase))
        {
            matchingStatus = statuses.FirstOrDefault(s =>
                s.Name.Contains("Tech", StringComparison.OrdinalIgnoreCase) &&
                (s.Name.Contains("interv", StringComparison.OrdinalIgnoreCase) || s.Name.Contains("inerv", StringComparison.OrdinalIgnoreCase)))
                ?? statuses.FirstOrDefault(s => s.Name.Contains("Tech", StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            matchingStatus = statuses.FirstOrDefault(s => s.Name.Equals(typeKeyword, StringComparison.OrdinalIgnoreCase))
                ?? statuses.FirstOrDefault(s => s.Name.Contains(typeKeyword, StringComparison.OrdinalIgnoreCase) || typeKeyword.Contains(s.Name, StringComparison.OrdinalIgnoreCase));
        }

        if (matchingStatus is not null)
        {
            candidate.StatusId = matchingStatus.Id;
            candidate.Status = matchingStatus;
            _uow.Candidates.Update(candidate);
        }

        await _uow.SaveChangesAsync();

        // Re-fetch to get updated rankings/scores
        var updated = await _uow.Candidates.GetByIdWithDetailsAsync(id);
        var rankInfo = await _uow.Candidates.GetRankInJobAsync(id, updated!.JobId);
        var (cvBaseUrl, profileBaseUrl) = await GetBaseUrlsAsync();

        return Ok(ApiResponse<CandidateDetailDto>.SuccessResponse(
            updated.ToDetailDto(rankInfo.Rank, rankInfo.TotalCandidates, cvBaseUrl, profileBaseUrl), "Interview grade updated."));
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
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse($"Can not delete this one {ex.Message}"));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Deleted successfully"));
    }

    // ── CV extraction endpoint ─────────────────────────────────────────────────

    /// <summary>
    /// Extract structured data from a candidate's CV PDF file.
    /// Returns: Skills, Experience, Languages, Certificates, Education.
    /// </summary>
    /// <param name="id">Candidate ID.</param>
    [HttpGet("{id:int}/extract-cv")]
    public async Task<ActionResult<ApiResponse<CvExtractedDataDto>>> ExtractCv(int id)
    {
        var candidate = await _uow.Candidates.GetByIdWithDetailsAsync(id);
        if (candidate is null)
            return NotFound(ApiResponse<CvExtractedDataDto>.NotFoundResponse("Candidate not found."));

        if (string.IsNullOrWhiteSpace(candidate.CvFile))
            return BadRequest(ApiResponse<CvExtractedDataDto>.ErrorResponse("This candidate does not have a CV file uploaded."));

        // Resolve physical path: PhysicalBasePath / CvFile / filename
        var physicalBase = _config["FileUpload:PhysicalBasePath"] ?? "Uploads";
        if (!Path.IsPathRooted(physicalBase))
            physicalBase = Path.Combine(_env.ContentRootPath, physicalBase);

        var physicalPath = Path.Combine(physicalBase, CvConfigurationKey, candidate.CvFile);

        if (!System.IO.File.Exists(physicalPath))
            return NotFound(ApiResponse<CvExtractedDataDto>.ErrorResponse($"CV file not found on disk: {candidate.CvFile}"));

        try
        {
            var result = _cvParserService.Extract(physicalPath, candidate.Id, candidate.Name);
            return Ok(ApiResponse<CvExtractedDataDto>.SuccessResponse(result, "CV data extracted successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<CvExtractedDataDto>.ErrorResponse($"Failed to parse CV: {ex.Message}"));
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>Fetches both the CV and Profile base URLs in a single async call.</summary>
    private async Task<(string? CvBaseUrl, string? ProfileBaseUrl)> GetBaseUrlsAsync()
    {
        var cvUrl      = (await _uow.Configurations.GetByKeyAsync(CvConfigurationKey))?.Value;
        var profileUrl = (await _uow.Configurations.GetByKeyAsync(ProfileConfigurationKey))?.Value;
        return (cvUrl, profileUrl);
    }

    private async Task<Status?> GetStatusByNameAsync(string name)
        => (await _uow.Statuses.GetAllAsync())
            .FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

    private async Task<Status> GetOrCreateStatusByNameAsync(string name)
    {
        var status = await GetStatusByNameAsync(name);
        if (status is not null)
            return status;

        status = new Status { Name = name };
        await _uow.Statuses.AddAsync(status);
        await _uow.SaveChangesAsync();
        return status;
    }

    private async Task<SecurityClearanceStatus?> GetSecurityClearanceByNameAsync(string name)
        => (await _uow.SecurityClearances.GetAllAsync())
            .FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Uploads a file to the specified folder key.
    /// Returns (FileName, null) on success or (null, errorMessage) on failure.
    /// </summary>
    private async Task<(string? FileName, string? Error)> UploadFileAsync(IFormFile file, string folderKey)
    {
        try
        {
            var upload = await _fileUploadService.UploadAsync(file, folderKey);
            return (upload.FileName, null);
        }
        catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException)
        {
            return (null, ex.Message);
        }
    }

    // ── Pipeline endpoint ─────────────────────────────────────────────────────

    /// <summary>
    /// Returns the hiring pipeline progress for a candidate.
    /// Steps: Application → HR Evaluation → Technical Evaluation → Offer → Security Clearance → Hired.
    /// Each step returns: stepName, completed (bool), status (Completed/InProgress/Pending), date, detail.
    /// If the candidate is Hired, all steps are forced to Completed.
    /// </summary>
    [HttpGet("{id:int}/pipeline")]
    public async Task<ActionResult<ApiResponse<CandidatePipelineDto>>> GetPipeline(int id)
    {
        var candidate = await _uow.Candidates.GetByIdWithDetailsAsync(id);
        if (candidate is null)
            return NotFound(ApiResponse<CandidatePipelineDto>.NotFoundResponse());

        // Load all offers for this candidate
        var offers = (await _uow.Offers.GetByCandidateAsync(id)).ToList();

        var isHired = candidate.HiringDate.HasValue &&
                      (string.Equals(candidate.Status?.Name, HiredStatusName,
                                    StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(candidate.Status?.Name, SignContractStatusName,
                                    StringComparison.OrdinalIgnoreCase));

        var steps = BuildPipeline(candidate, offers, isHired);

        return Ok(ApiResponse<CandidatePipelineDto>.SuccessResponse(new CandidatePipelineDto(
            CandidateId:   candidate.Id,
            CandidateName: candidate.Name,
            IsHired:       isHired,
            Steps:         steps
        )));
    }

    /// <summary>
    /// Returns a ranked list of all candidates for a specific job.
    /// Ordered by their Overall Score (descending).
    /// </summary>
    [HttpGet("job/{jobId:int}/ranking")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CandidateRankingDto>>>> GetJobRanking(int jobId)
    {
        if (!await _uow.Jobs.ExistsAsync(jobId))
            return NotFound(ApiResponse<IEnumerable<CandidateRankingDto>>.ErrorResponse($"Job {jobId} not found."));

        var candidates = await _uow.Candidates.GetJobRankingAsync(jobId);
        var (cvBaseUrl, profileBaseUrl) = await GetBaseUrlsAsync();

        var rankedList = candidates.Select((c, index) => 
        {
            var score = CandidateMapper.ComputeOverallScore(c.Interviews);
            
            return new CandidateRankingDto(
                Rank: index + 1,
                CandidateId: c.Id,
                CandidateName: c.Name,
                CandidateCode: CandidateMapper.BuildCandidateCode(c.Id),
                ProfileImage: CandidateMapper.BuildFileUrl(c.Profile, profileBaseUrl),
                OverallScore: score.HasValue ? Math.Round(score.Value, 2) : null
            );
        });

        return Ok(ApiResponse<IEnumerable<CandidateRankingDto>>.SuccessResponse(rankedList));
    }

    /// <summary>
    /// Builds the 7-step pipeline list from the candidate and their offers.
    /// </summary>
    private static List<CandidatePipelineStepDto> BuildPipeline(
        Candidate           candidate,
        List<Offer>         offers,
        bool                isHired)
    {
        // ── Helper: find interview by type keyword ────────────────────────────
        Interview? GetInterview(string keyword) =>
            candidate.Interviews
                .FirstOrDefault(i => i.Type?.Name != null &&
                                     i.Type.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));

        bool HasGrade(Interview? i) =>
            i is not null && !string.IsNullOrWhiteSpace(i.Grade);

        var isContractSigned = isHired || string.Equals(
            candidate.Status?.Name, SignContractStatusName, StringComparison.OrdinalIgnoreCase);

        // ── Step 1: Application — always completed ────────────────────────────
        var stepApplication = new CandidatePipelineStepDto(
            StepName:  "Application",
            Completed: true,
            Status:    PipelineCompleted,
            Date:      null,
            Detail:    null
        );

        // ── Step 2: HR Evaluation ─────────────────────────────────────────────
        var hrInterview = GetInterview("HR");
        var hrCompleted = HasGrade(hrInterview);
        var stepHr = new CandidatePipelineStepDto(
            StepName:  "HR Evaluation",
            Completed: hrCompleted,
            Status:    hrCompleted             ? PipelineCompleted
                       : hrInterview is not null ? PipelineInProgress
                                                 : PipelinePending,
            Date:      hrInterview?.CreatedAt,
            Detail:    hrInterview?.Grade
        );

        // ── Step 3: Technical Evaluation ─────────────────────────────────────
        var techInterview = GetInterview("Technical");
        var techCompleted = HasGrade(techInterview);
        var stepTech = new CandidatePipelineStepDto(
            StepName:  "Technical Evaluation",
            Completed: techCompleted,
            Status:    techCompleted              ? PipelineCompleted
                       : techInterview is not null ? PipelineInProgress
                                                   : PipelinePending,
            Date:      techInterview?.CreatedAt,
            Detail:    techInterview?.Grade
        );

        // ── Step 4: Offer ─────────────────────────────────────────────────────
        var latestOffer    = offers.FirstOrDefault();
        var offerCompleted = latestOffer?.OfferStatusId == OfferStatus.Accepted;
        var stepOffer = new CandidatePipelineStepDto(
            StepName:  "Offer",
            Completed: offerCompleted,
            Status:    offerCompleted                                         ? PipelineCompleted
                       : latestOffer is not null &&
                         latestOffer.OfferStatusId == OfferStatus.Pending      ? PipelineInProgress
                                                                               : PipelinePending,
            Date:      latestOffer?.OfferDate,
            Detail:    latestOffer?.OfferStatusId.ToString()
        );

        // ── Step 5: Security Clearance ────────────────────────────────────────
        var clearanceName      = candidate.SecurityClearance?.Name ?? string.Empty;
        var clearanceCompleted = !string.Equals(
            clearanceName, InitialSecurityClearanceName, StringComparison.OrdinalIgnoreCase);
        var stepClearance = new CandidatePipelineStepDto(
            StepName:  "Security Clearance",
            Completed: clearanceCompleted,
            Status:    clearanceCompleted ? PipelineCompleted : PipelinePending,
            Date:      null,
            Detail:    clearanceName
        );

        // ── Step 6: Contract signed ───────────────────────────────────────────
        var stepContractSigned = new CandidatePipelineStepDto(
            StepName:  "ContractedSign",
            Completed: isContractSigned,
            Status:    isContractSigned ? PipelineCompleted : PipelinePending,
            Date:      null,
            Detail:    isContractSigned ? "SignContract" : null
        );

        // ── Step 7: Hired ─────────────────────────────────────────────────────
        var stepHired = new CandidatePipelineStepDto(
            StepName:  "Hired",
            Completed: isHired,
            Status:    isHired ? PipelineCompleted : PipelinePending,
            Date:      candidate.HiringDate,
            Detail:    isHired ? "Hired" : null
        );

        // ── If hired — force all steps completed ──────────────────────────────
        if (isHired)
        {
            return
            [
                stepApplication with { Completed = true, Status = PipelineCompleted },
                stepHr          with { Completed = true, Status = PipelineCompleted },
                stepTech        with { Completed = true, Status = PipelineCompleted },
                stepOffer       with { Completed = true, Status = PipelineCompleted },
                stepClearance   with { Completed = true, Status = PipelineCompleted },
                stepContractSigned with { Completed = true, Status = PipelineCompleted },
                stepHired
            ];
        }

        return [ stepApplication, stepHr, stepTech, stepOffer, stepClearance, stepContractSigned, stepHired ];
    }
}
