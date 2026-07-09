using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Enums;
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
    private const string CvConfigurationKey           = "CvFile";
    private const string ProfileConfigurationKey       = "ProfileImage";
    private const string InitialStatusName             = "Under Vetting";
    private const string InitialSecurityClearanceName  = "In Check";
    // Pipeline status names
    private const string HiredStatusName               = "Hired";
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

        var (cvBaseUrl, profileBaseUrl) = await GetBaseUrlsAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)safePageSize);

        var result = new PagedResult<CandidateDto>(
            items.Select(c => c.ToDto(cvBaseUrl, profileBaseUrl)),
            totalCount,
            safePage,
            safePageSize,
            totalPages);

        return Ok(ApiResponse<PagedResult<CandidateDto>>.SuccessResponse(result));
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
        };

        await _uow.Candidates.AddAsync(entity);
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

        var entity = await _uow.Candidates.GetByIdAsync(id);
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

        _uow.Candidates.Update(entity);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Candidates.GetByIdWithDetailsAsync(entity.Id);
        var (cvBaseUrl, profileBaseUrl) = await GetBaseUrlsAsync();
        return Ok(ApiResponse<CandidateDto>.SuccessResponse(updated!.ToDto(cvBaseUrl, profileBaseUrl)));
    }

    /// <summary>Update only the application status for a candidate (e.g. Under Vetting, Approved, Rejected).</summary>
    /// <param name="id">Candidate ID.</param>
    /// <param name="dto">New status ID.</param>
    /// <returns>The updated candidate.</returns>
    /// 
    [HttpGet("statuses")]
    public async Task<ActionResult<ApiResponse<IEnumerable<StatusDto>>>> GetStatuses()
    {
        var list = await _uow.Statuses.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<StatusDto>>.SuccessResponse(list.Select(s => s.ToDto())));
    }
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<ApiResponse<CandidateDto>>> PatchStatus(
        int id, [FromBody] PatchCandidateStatusDto  dto)
    {
        var entity = await _uow.Candidates.GetByIdAsync(id);
        if (entity is null) return NotFound(ApiResponse<CandidateDto>.NotFoundResponse());

        if (!await _uow.Statuses.ExistsAsync(dto.StatusId))
            return BadRequest(ApiResponse<CandidateDto>.ErrorResponse($"Status {dto.StatusId} not found."));

        entity.StatusId = dto.StatusId;
        _uow.Candidates.Update(entity);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Candidates.GetByIdWithDetailsAsync(entity.Id);
        var (cvBaseUrl, profileBaseUrl) = await GetBaseUrlsAsync();
        return Ok(ApiResponse<CandidateDto>.SuccessResponse(updated!.ToDto(cvBaseUrl, profileBaseUrl), "Candidate status updated."));
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
    [HttpPatch("{id:int}/grade")]
    public async Task<ActionResult<ApiResponse<CandidateDetailDto>>> PatchGrade(
        int id, [FromBody] PatchCandidateInterviewGradeDto dto)
    {
        var candidate = await _uow.Candidates.GetByIdWithDetailsAsync(id);
        if (candidate is null) return NotFound(ApiResponse<CandidateDetailDto>.NotFoundResponse());

        var typeKeyword = dto.InterviewType.Trim();
        var interview = candidate.Interviews.FirstOrDefault(i =>
            i.Type?.Name != null &&
            i.Type.Name.Contains(typeKeyword, StringComparison.OrdinalIgnoreCase));

        if (interview is not null)
        {
            interview.Grade = dto.Grade;
            if (dto.Comments is not null)
                interview.Comments = dto.Comments;
            _uow.Interviews.Update(interview);
        }
        else
        {
            var types = await _uow.InterviewTypes.GetAllAsync();
            var type = types.FirstOrDefault(t => t.Name.Contains(typeKeyword, StringComparison.OrdinalIgnoreCase));
            if (type is null)
                return BadRequest(ApiResponse<CandidateDetailDto>.ErrorResponse($"Interview type containing '{typeKeyword}' not found."));

            interview = new Interview
            {
                CandidateId = candidate.Id,
                TypeId = type.Id,
                Grade = dto.Grade,
                Comments = dto.Comments,
                CreatedAt = DateTime.UtcNow
            };
            await _uow.Interviews.AddAsync(interview);
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

        var isHired = candidate.HiringDate.HasValue ||
                      string.Equals(candidate.Status?.Name, HiredStatusName,
                                    StringComparison.OrdinalIgnoreCase);

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
    /// Builds the 6-step pipeline list from the candidate and their offers.
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

        // ── Step 6: Hired ─────────────────────────────────────────────────────
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
                stepHired
            ];
        }

        return [ stepApplication, stepHr, stepTech, stepOffer, stepClearance, stepHired ];
    }
}
