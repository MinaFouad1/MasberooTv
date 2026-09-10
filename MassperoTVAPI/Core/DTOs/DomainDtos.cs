using System.ComponentModel.DataAnnotations;
using MassperoTVAPI.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace MassperoTVAPI.Core.DTOs;

// ── Security Clearance Status ─────────────────────────────────────────────────
public record SecurityClearanceStatusDto(int Id, string Name);
public record CreateSecurityClearanceStatusDto
{
    [Required] [MaxLength(100)] public string Name { get; init; } = string.Empty;
}
public record UpdateSecurityClearanceStatusDto
{
    [Required] [MaxLength(100)] public string Name { get; init; } = string.Empty;
}

// ── Candidate Status ──────────────────────────────────────────────────────────
public record StatusDto(int Id, string Name);
public record CreateStatusDto
{
    [Required] [MaxLength(100)] public string Name { get; init; } = string.Empty;
}
public record UpdateStatusDto
{
    [Required] [MaxLength(100)] public string Name { get; init; } = string.Empty;
}

// ── Category ──────────────────────────────────────────────────────────────────
public record CategoryDto(
    int Id,
    string Name,
    string? CategoryCode,
    bool IsActive,
    DateTime CreatedAt,
    int JobsCount
);

public record CategoryDetailsDto(
    int Id,
    string Name,
    string? CategoryCode,
    string? Description,
    bool IsActive,
    string Status,
    DateTime CreatedAt,
    int OpenPositions,
    int Applications,
    int Interviews,
    int OffersSent
);

public record CategorySummaryDto(
    int CategoryId,
    string CategoryName,
    string? CategoryCode,
    string? Description,
    bool IsActive,
    string Status,
    DateTime CreatedAt,
    int OpenPositions,
    int Applications,
    int Interviews,
    int OffersSent
);

public record CreateCategoryDto
{
    [Required] [MaxLength(150)] public string Name { get; init; } = string.Empty;
    [MaxLength(50)] public string? CategoryCode { get; init; }
     public string? CategoryDec { get; init; }
    public bool IsActive { get; init; } = true;
}

public record UpdateCategoryDto
{
    [Required] [MaxLength(150)] public string Name { get; init; } = string.Empty;
    [MaxLength(50)] public string? CategoryCode { get; init; }
    public bool IsActive { get; init; } = true;
}

// ── InterviewType ─────────────────────────────────────────────────────────────
public record InterviewTypeDto(int Id, string Name);
public record CreateInterviewTypeDto
{
    [Required] [MaxLength(100)] public string Name { get; init; } = string.Empty;
}
public record UpdateInterviewTypeDto
{
    [Required] [MaxLength(100)] public string Name { get; init; } = string.Empty;
}

// ── Phase ─────────────────────────────────────────────────────────────────────
public record PhaseDto(int Id, string Name);
public record CreatePhaseDto
{
    [Required] [MaxLength(150)] public string Name { get; init; } = string.Empty;
}
public record UpdatePhaseDto
{
    [Required] [MaxLength(150)] public string Name { get; init; } = string.Empty;
}

// ── ProcessStatus ─────────────────────────────────────────────────────────────
public record CreateProcessStatusDto
{
    [Required] [MaxLength(50)] public string Name       { get; init; } = string.Empty;
    [Range(0, 100)]            public decimal Percentage { get; init; }
}
public record UpdateProcessStatusDto
{
    [Required] [MaxLength(50)] public string Name       { get; init; } = string.Empty;
    [Range(0, 100)]            public decimal Percentage { get; init; }
}

// ── Job ───────────────────────────────────────────────────────────────────────
public record JobDto(
    int       Id,
    string    Name,
    string?   JobDescription,
    int       CategoryId,
    string    CategoryName,
    int       OpenPositions,
    DateTime  CreatedAt,
    string?   EmploymentType,
    DateTime? TargetHiringDate,
    DateTime? DeadLineDate,
    bool      IsOpend,
    string    Status,
    int?      LocationId,
    string?   LocationName,
    string?   HiringManagerId,
    string?   HiringManagerName,
    int       Applicants,
    int       Interviews,
    int       Offers
);

public record JobRecruitmentProgressDto(
    int    JobId,
    string JobName,
    int    Applications,
    int    HrInterviews,
    int    TechnicalInterviews,
    int    OffersSent,
    int    Accepted,
    int    Hired,
    int    TotalApplications
);

public record CreateJobDto
{
    [Required] [MaxLength(200)] public string    Name             { get; init; } = string.Empty;
    [MaxLength(1000)]           public string?   JobDescription   { get; init; }
    [Required]                  public int       CategoryId       { get; init; }
                                public int       OpenPositions    { get; init; } = 0;
    [MaxLength(100)]            public string?   EmploymentType   { get; init; }
                                public DateTime? TargetHiringDate { get; init; }
                                public DateTime? DeadLineDate     { get; init; }
                                public bool      IsOpend          { get; init; } = true;
                                public int?      LocationId       { get; init; }
                                public string?   HiringManagerId  { get; init; }
}
public record UpdateJobDto
{
    [Required] [MaxLength(200)] public string    Name             { get; init; } = string.Empty;
    [MaxLength(1000)]           public string?   JobDescription   { get; init; }
    [Required]                  public int       CategoryId       { get; init; }
                                public int       OpenPositions    { get; init; } = 0;
    [MaxLength(100)]            public string?   EmploymentType   { get; init; }
                                public DateTime? TargetHiringDate { get; init; }
                                public DateTime? DeadLineDate     { get; init; }
                                public bool      IsOpend          { get; init; } = true;
                                public int?      LocationId       { get; init; }
                                public string?   HiringManagerId  { get; init; }
}

// ── Candidate ─────────────────────────────────────────────────────────────────
public record CandidateDto(
    int       Id,
    string    Name,
    string?   CvFile,
    string?   ProfileImage,
    string?   ReasonOfAccept,
    string?   ReasonOfReject,
    bool?     Accepted,
    int       JobId,
    string    JobName,
    int       CategoryId,
    string    CategoryName,
    int       StatusId,
    string    StatusName,
    int       SecurityClearanceId,
    string    SecurityClearanceName,
    string?   ApplicationUserId,
    string?   ApplicationUserName,
    DateTime? HiringDate,
    string?   CurrentEmployer,
    string?   CurrentPosition,
    int?      YearsOfExperience,
    decimal?  ExpectedSalary,
    string?   NoticePeriod,
    string?   Availability,
    decimal?  CurrentSalary,
    string?   Summary,
    IEnumerable<CandidateSkillDto> Skills,
    IEnumerable<CandidateLanguageDto> Languages,
    IEnumerable<CandidateCertificationDto> Certifications,
    IEnumerable<CandidateEducationDto> Educations,
    string?   HrScore,
    string?   TechnicalScore
);
public record CandidateSkillDto(int Id, int SkillId, string Name, int YearsOfExperience);
public record CandidateLanguageDto(int Id, int LanguageId, string Name);
public record CandidateCertificationDto(int Id, string Name, string? Url);
public record CandidateEducationDto(int Id, string Degree, string University, int GraduationYear, string? Grade);

public record ApplicantDto(
    int       Id,
    string    Candidate,
    string?   ProfileImage,
    int       JobId,
    string    AppliedJob,
    int       CategoryId,
    string    Category,
    DateTime? AppliedDate,
    string    Stage,
    string?   HrScore,
    string?   TechnicalScore,
    string    Status
);

public record UpsertCandidateSkillDto
{
    [Required] [MaxLength(150)] public string Name { get; init; } = string.Empty;
    [Range(0, 100)] public int YearsOfExperience { get; init; }
}
public record UpsertCandidateLanguageDto
{
    [Required] [MaxLength(100)] public string Name { get; init; } = string.Empty;
}
public record UpsertCandidateCertificationDto
{
    [Required] [MaxLength(200)] public string Name { get; init; } = string.Empty;
    [MaxLength(500)] public string? Url { get; init; }
}
public record UpsertCandidateEducationDto
{
    [Required] [MaxLength(200)] public string Degree { get; init; } = string.Empty;
    [Required] [MaxLength(200)] public string University { get; init; } = string.Empty;
    public int GraduationYear { get; init; }
    [MaxLength(100)] public string? Grade { get; init; }
}
public record CreateCandidateDto
{
    [Required] [MaxLength(200)] public string    Name         { get; init; } = string.Empty;
    public IFormFile?                            CvFile       { get; init; }
    public IFormFile?                            ProfileImage { get; init; }
    [Required]                  public int       JobId        { get; init; }
    [MaxLength(200)]            public string?   CurrentEmployer    { get; init; }
    [MaxLength(200)]            public string?   CurrentPosition    { get; init; }
                                public int?      YearsOfExperience  { get; init; }
                                public decimal?  ExpectedSalary     { get; init; }
    [MaxLength(100)]            public string?   NoticePeriod       { get; init; }
    [MaxLength(100)]            public string?   Availability       { get; init; }
                                public decimal?  CurrentSalary      { get; init; }
    public string?              Summary { get; init; }

    // ── Personal info (all optional) ──────────────────────────────────────────
    public Gender?         Gender               { get; init; }
    [MaxLength(50)]  public string?   NationalId           { get; init; }
    [MaxLength(500)] public string?   Address              { get; init; }
    [MaxLength(20)]  public string?   Mobile               { get; init; }
    [MaxLength(20)]  public string?   AlternateMobile      { get; init; }
    [EmailAddress]
    [MaxLength(200)] public string?   Email                { get; init; }
    public MaritalStatus?  MaritalStatus        { get; init; }
                     public DateTime? DateOfBeginning      { get; init; }
    [MaxLength(100)] public string?   Nationality          { get; init; }
    [MaxLength(200)] public string?   PreferredJobLocation { get; init; }
    public PreferredShift? PreferredShift       { get; init; }
    [MaxLength(100)] public string?   City                 { get; init; }
    public Country?        Country              { get; init; }

    public List<UpsertCandidateSkillDto>?         Skills             { get; init; }
    public List<UpsertCandidateLanguageDto>?      Languages          { get; init; }
    public List<UpsertCandidateCertificationDto>? Certifications     { get; init; }
    public List<UpsertCandidateEducationDto>?     Educations         { get; init; }
}
public record UpdateCandidateDto
{
    [Required] [MaxLength(200)] public string    Name           { get; init; } = string.Empty;
    public IFormFile?                            CvFile         { get; init; }
    public IFormFile?                            ProfileImage   { get; init; }
    [MaxLength(1000)]           public string?   ReasonOfAccept { get; init; }
    [MaxLength(1000)]           public string?   ReasonOfReject { get; init; }
    [Required]                  public int       JobId          { get; init; }
    [MaxLength(200)]            public string?   CurrentEmployer    { get; init; }
    [MaxLength(200)]            public string?   CurrentPosition    { get; init; }
                                public int?      YearsOfExperience  { get; init; }
                                public decimal?  ExpectedSalary     { get; init; }
    [MaxLength(100)]            public string?   NoticePeriod       { get; init; }
    [MaxLength(100)]            public string?   Availability       { get; init; }
                                public decimal?  CurrentSalary      { get; init; }
    public string?              Summary { get; init; }

    // ── Personal info (all optional) ──────────────────────────────────────────
    public Gender?         Gender               { get; init; }
    [MaxLength(50)]  public string?   NationalId           { get; init; }
    [MaxLength(500)] public string?   Address              { get; init; }
    [MaxLength(20)]  public string?   Mobile               { get; init; }
    [MaxLength(20)]  public string?   AlternateMobile      { get; init; }
    [EmailAddress]
    [MaxLength(200)] public string?   Email                { get; init; }
    public MaritalStatus?  MaritalStatus        { get; init; }
                     public DateTime? DateOfBeginning      { get; init; }
    [MaxLength(100)] public string?   Nationality          { get; init; }
    [MaxLength(200)] public string?   PreferredJobLocation { get; init; }
    public PreferredShift? PreferredShift       { get; init; }
    [MaxLength(100)] public string?   City                 { get; init; }
    public Country?        Country              { get; init; }

    public List<UpsertCandidateSkillDto>?         Skills             { get; init; }
    public List<UpsertCandidateLanguageDto>?      Languages          { get; init; }
    public List<UpsertCandidateCertificationDto>? Certifications     { get; init; }
    public List<UpsertCandidateEducationDto>?     Educations         { get; init; }
}

public record PatchCandidateInterviewGradeDto
{
    [Required]                  public string    InterviewType  { get; init; } = string.Empty; // e.g. "HR" or "Technical"
    [MaxLength(50)]             public string?   Grade          { get; init; }
    [MaxLength(2000)]           public string?   Comments       { get; init; }
}

// ── Candidate Detail (GET by ID) ──────────────────────────────────────────────

public record CandidateRankingDto(
    int       Rank,
    int       CandidateId,
    string    CandidateName,
    string    CandidateCode,
    string?   ProfileImage,
    decimal?  OverallScore
);

/// <summary>Single interview summary used inside CandidateDetailDto.</summary>
public record InterviewSummaryDto(
    int     Id,
    string  TypeName,
    string? Grade,
    string? Comments,
    string? EvaluatorId,
    DateTime CreatedAt
);

/// <summary>
/// Rich single-candidate response for GET /api/candidates/{id}.
/// Includes computed analytics: overall score, ranking, hiring probability.
/// </summary>
public record CandidateDetailDto(
    // ── Identity ──────────────────────────────────────────────────────────────
    int       Id,
    string    Name,
    string    CandidateCode,

    // ── Files ─────────────────────────────────────────────────────────────────
    string?   CvFile,
    string?   ProfileImage,

    // ── Job / position ────────────────────────────────────────────────────────
    int       JobId,
    string    JobName,
    string?   JobDescription,
    string?   EmploymentType,
    int       CategoryId,
    string    CategoryName,
    string?   WorkLocation,
    string?   Summary,
    DateTime  JobCreatedAt,
    DateTime? TargetHiringDate,

    // ── Status & dates ────────────────────────────────────────────────────────
    int       StatusId,
    string    StatusName,
    int       SecurityClearanceId,
    string    SecurityClearanceName,
    DateTime? HiringDate,

    // ── Assigned HR user ──────────────────────────────────────────────────────
    string?   ApplicationUserId,
    string?   ApplicationUserName,

    // ── Candidate notes ───────────────────────────────────────────────────────
    string?   ReasonOfAccept,
    string?   ReasonOfReject,
    bool?     Accepted,
    string?   CurrentEmployer,
    string?   CurrentPosition,
    int?      YearsOfExperience,
    decimal?  ExpectedSalary,
    string?   NoticePeriod,
    string?   Availability,
    decimal?  CurrentSalary,

    // ── Personal info ─────────────────────────────────────────────────────────
    Gender?        Gender,
    string?        NationalId,
    string?        Address,
    string?        Mobile,
    string?        AlternateMobile,
    string?        Email,
    MaritalStatus? MaritalStatus,
    DateTime?      DateOfBeginning,
    string?        Nationality,
    string?        PreferredJobLocation,
    PreferredShift? PreferredShift,
    string?        City,
    Country?       Country,

    IEnumerable<CandidateSkillDto> Skills,
    IEnumerable<CandidateLanguageDto> Languages,
    IEnumerable<CandidateCertificationDto> Certifications,
    IEnumerable<CandidateEducationDto> Educations,

    // ── Interview scores ──────────────────────────────────────────────────────
    string?   HrScore,
    string?   TechnicalScore,
    decimal?  OverallScore,        // average of all parseable interview grades
    string    HiringProbability,   // "Low" | "Medium" | "High" | "Very High" | "N/A"

    // ── Ranking ───────────────────────────────────────────────────────────────
    int       RankingPosition,     // 1-based; rank among all candidates for same job

    // ── All interviews ────────────────────────────────────────────────────────
    IEnumerable<InterviewSummaryDto> Interviews
);

// ── Interview ─────────────────────────────────────────────────────────────────
public record InterviewDto(
    int     Id,
    string? Grade,
    string? Comments,
    DateTime CreatedAt,
    string? EvaluatorId,
    string? EvaluatorName,
    int     CandidateId,
    string  CandidateName,
    int     TypeId,
    string  TypeName,
    string? InterviewMode,
    DateTime? InterviewDate,
    TimeOnly? StartTime,
    TimeOnly? EndTime
);
public record CreateInterviewDto
{
    [MaxLength(50)]   public string? Grade       { get; init; }
    [MaxLength(2000)] public string? Comments    { get; init; }
    [Required]        public int     CandidateId { get; init; }
    [Required]        public int     TypeId      { get; init; }
}
public record UpdateInterviewDto
{
    [MaxLength(50)]   public string? Grade       { get; init; }
    [MaxLength(2000)] public string? Comments    { get; init; }
    [Required]        public int     CandidateId { get; init; }
    [Required]        public int     TypeId      { get; init; }
}

/// <summary>Schedules an interview and emails its details to the candidate.</summary>
public record ScheduleInterviewDto
{
    [Required] public int CandidateId { get; init; }
    [Required] public int TypeId { get; init; }
    [Required]
    [RegularExpression("^(Online|Offline)$", ErrorMessage = "InterviewMode must be Online or Offline.")]
    public string InterviewMode { get; init; } = string.Empty;
    [Required] public DateTime InterviewDate { get; init; }
    [Required] public TimeOnly StartTime { get; init; }
    [Required] public TimeOnly EndTime { get; init; }
}

// ── Issue ─────────────────────────────────────────────────────────────────────
public record IssueDetailDto(
    int Id,
    string Name,
    IssueStatus Status,
    DateTime? Date,
    DateTime? DueDate,
    IssuePriority? Priority,
    int ProcessId,
    string ProcessName,
    string? CreatedByUserId,
    string? CreatedByUserName
);

public record RiskResponseDto(
    int RiskId,
    string RiskTitle,
    string? Priority,
    string Status,
    DateTime? Date,
    DateTime? DueDate,
    string? CreatedByUser
);
public record CreateIssueDto
{
    [Required] [MaxLength(300)] public string         Name      { get; init; } = string.Empty;
    [Required]                  public int            ProcessId { get; init; }
    public IssueStatus?   Status    { get; init; }
    public DateTime?      Date      { get; init; }
    public DateTime?      DueDate   { get; init; }
    public IssuePriority? Priority  { get; init; }
}
public record UpdateIssueDto
{
    [Required] [MaxLength(300)] public string         Name      { get; init; } = string.Empty;
    [Required]                  public int            ProcessId { get; init; }
    public IssueStatus?   Status    { get; init; }
    public DateTime?      Date      { get; init; }
    public DateTime?      DueDate   { get; init; }
    public IssuePriority? Priority  { get; init; }
}
public record PatchIssueStatusDto
{
    [Required] public IssueStatus Status { get; init; }
}

// ── Roles ─────────────────────────────────────────────────────────────────────
public record RoleDto(string Id, string Name);
public record CreateRoleDto
{
    [Required] [MaxLength(100)] public string Name { get; init; } = string.Empty;
}
public record UpdateRoleDto
{
    [Required] [MaxLength(100)] public string Name { get; init; } = string.Empty;
}
public record AssignRoleDto
{
    [Required] public string UserId   { get; init; } = string.Empty;
    [Required] public string RoleName { get; init; } = string.Empty;
}
public record RemoveRoleDto
{
    [Required] public string UserId   { get; init; } = string.Empty;
    [Required] public string RoleName { get; init; } = string.Empty;
}

// ── Candidate patch-status DTOs ───────────────────────────────────────────────
/// <summary>PATCH body to update only the candidate application status.</summary>
public record PatchCandidateStatusDto
{
    [Required] public int StatusId { get; init; }
}

/// <summary>PATCH body to update only the candidate security clearance status.</summary>
public record PatchCandidateSecurityDto
{
    [Required] public int SecurityClearanceId { get; init; }
}

// ── User management DTOs (Admin) ──────────────────────────────────────────────
public record UserDto(
    string Id,
    string UserName,
    string PhoneNumber,
    string Email,
    bool IsVerified,
    int? Failed_Login_Attempts,
    IList<string> Roles,
    AccountStatus AccountStatus,
    DateTime? LastLoginAt
);

public record CreateHrUserDto
{
    [Required] [EmailAddress]
    public string Email    { get; init; } = string.Empty;

    [Required] [MinLength(3)]
    public string UserName { get; init; } = string.Empty;

    [Required] [MinLength(8)]
    public string Password { get; init; } = string.Empty;
}

/// <summary>Payload for admin to change a user's account status.</summary>
public record UpdateUserStatusDto
{
    [Required(ErrorMessage = "Status is required.")]
    public AccountStatus Status { get; init; }
}

/// <summary>
/// Flat search-result DTO returned by GET /api/users/search.
/// Contains only the fields relevant for the user list view.
/// </summary>
public record UserSearchResultDto(
    string Id,
    string UserName,
    string Email,
    IList<string> Roles,
    AccountStatus AccountStatus,
    DateTime? LastLoginAt
);

/// <summary>Query parameters for the advanced user-search endpoint.</summary>
public record UserSearchQueryDto
{
    /// <summary>Partial match on username (case-insensitive).</summary>
    public string? UserName { get; init; }

    /// <summary>Partial match on email (case-insensitive).</summary>
    public string? Email { get; init; }

    /// <summary>Exact match on role name (e.g. "Admin", "HR").</summary>
    public string? Role { get; init; }

    /// <summary>Filter by account status (Active / Blocked / Locked).</summary>
    public AccountStatus? Status { get; init; }

    /// <summary>
    /// How to combine the provided filters.
    /// "AND" (default) — user must satisfy ALL supplied filters.
    /// "OR"            — user must satisfy AT LEAST ONE supplied filter.
    /// </summary>
    public string Mode { get; init; } = "AND";
}

// ── Admin dashboard statistics ─────────────────────────────────────────────────
public record AdminDashboardDto(
    int TotalCandidates,
    int TotalJobs,
    int TotalUsers,
    int TotalInterviews,
    IEnumerable<StatusCountDto>            CandidatesByStatus,
    IEnumerable<StatusCountDto>            CandidatesBySecurityClearance,
    IEnumerable<StatusCountDto>            CandidatesByJob,
    IEnumerable<ProcessPhaseProgressDto>   ProcessProgress
);

public record StatusCountDto(string Name, int Count);

// ── Process / Phase dashboard DTOs (Client) ────────────────────────────────────
public record ProcessDashboardDto(
    decimal                              OverallProgress,
    IEnumerable<ProcessPhaseProgressDto> Phases
);

public record ProcessPhaseProgressDto(
    int     PhaseId,
    string  PhaseName,
    int     TotalProcesses,
    decimal AverageCompletion,
    IEnumerable<ProcessSummaryDto> Processes
);

public record ProcessSummaryDto(
    int      Id,
    string   Name,
    string   StatusName,
    decimal  Percentage,
    DateTime StartDate,
    DateTime? EndDate,
    int      IssueCount,
    int      DependencyCount
);

public record ProcessStatisticsDto(
    int TotalProcesses,
    int ActiveProcesses,
    int CompletedProcesses,
    decimal AverageCompletion,
    IEnumerable<StatusCountDto> ByStatus,
    IEnumerable<StatusCountDto> ByPhase
);

public record IssueStatisticsDto(
    int TotalIssues,
    int SolvedIssues,
    int OpenIssues,
    IEnumerable<StatusCountDto> ByStatus,
    IEnumerable<StatusCountDto> ByProcess,
    IEnumerable<StatusCountDto> ByPriority
);

public record PhaseStatisticsDto(
    int TotalPhases,
    IEnumerable<ProcessPhaseProgressDto> Phases
);

public record PhaseSummaryItemDto(
    int Id,
    string Name,
    decimal AverageCompletion,
    DateTime? StartDate,
    DateTime? EndDate,
    int TotalProcesses
);

public record PhasesSummaryDto(
    int TotalPhases,
    IEnumerable<PhaseSummaryItemDto> Phases
);

// ── Recruitment Funnel ────────────────────────────────────────────────────────
public record RecruitmentFunnelDto(
    int CandidatesCount,
    int HrInterviewCount,
    int TechnicalInterviewCount,
    int OffersSent,
    int OffersAccepted,
    int Hired
);

// ── Job Status (Dashboard) ────────────────────────────────────────────────────
public record JobStatusDto(
    string JobTitle,
    int    OpenPositions,
    int    Applications
);

// ── Risk Management Issues (Dashboard) ───────────────────────────────────────
public record RiskIssueDto(
    int            Id,
    string         IssueName,
    IssuePriority? Priority,
    IssueStatus    Status,
    DateTime?      DueDate
);

public record RiskStatisticsDto(
    int TotalRisks,
    RiskPriorityStatDto Critical,
    RiskPriorityStatDto High,
    RiskPriorityStatDto Medium,
    RiskPriorityStatDto Low
);

public record RiskPriorityStatDto(
    int Count,
    decimal Percentage
);

// ── Job Statistics ────────────────────────────────────────────────────────────
public record JobStatisticsDto(
    int TotalJobs,
    int OpenPositions,
    int Applications,
    int OffersSent,
    int OffersAccepted
);

// ── Interview grade patch ─────────────────────────────────────────────────────
/// <summary>PATCH body to update the grade (and optionally comments) of an interview.</summary>
public record PatchInterviewGradeDto
{
    [Required] [MaxLength(50)]   public string  Grade    { get; init; } = string.Empty;
    [MaxLength(2000)]            public string? Comments { get; init; }
}

// ── Paged Result Wrapper ──────────────────────────────────────────────────────
/// <summary>Generic paged result returned by list endpoints that support pagination.</summary>
public record PagedResult<T>(
    IEnumerable<T> Items,
    int            TotalCount,
    int            Page,
    int            PageSize,
    int            TotalPages
);

/// <summary>Query parameters for searching/filtering candidates (applications) with pagination.</summary>
public class GetApplicationsQueryDto
{
    /// <summary>Filter by candidate name (partial match).</summary>
    public string?   CandidateName { get; init; }

    /// <summary>Filter by job ID.</summary>
    public int?      JobId         { get; init; }

    /// <summary>Filter by category ID (via Job.CategoryId).</summary>
    public int?      CategoryId    { get; init; }

    /// <summary>Filter by application status ID.</summary>
    public int?      StatusId      { get; init; }

    /// <summary>Filter by hiring date — from (inclusive).</summary>
    public DateTime? DateFrom      { get; init; }

    /// <summary>Filter by hiring date — to (inclusive).</summary>
    public DateTime? DateTo        { get; init; }

    /// <summary>Page number (1-based). Defaults to 1.</summary>
    public int       Page          { get; init; } = 1;

    /// <summary>Number of items per page. Defaults to 10.</summary>
    public int       PageSize      { get; init; } = 10;
}

/// <summary>Query parameters for searching/filtering categories with pagination.</summary>
public class GetCategoriesQueryDto
{
    /// <summary>Filter by category name (partial match).</summary>
    public string?   CategoryName { get; init; }

    /// <summary>Filter by active status.</summary>
    public bool?     IsActive     { get; init; }

    /// <summary>Page number (1-based). Defaults to 1.</summary>
    public int       Page         { get; init; } = 1;

    /// <summary>Number of items per page. Defaults to 10.</summary>
    public int       PageSize     { get; init; } = 10;
}

/// <summary>Query parameters for searching/filtering jobs with pagination.</summary>
public class GetJobsQueryDto
{
    /// <summary>Filter by job name / title (partial match).</summary>
    public string?   Search          { get; init; }

    /// <summary>Filter by category ID.</summary>
    public int?      CategoryId      { get; init; }

    /// <summary>Filter by creation date — created on or after this date.</summary>
    public DateTime? Date            { get; init; }

    /// <summary>Filter by location ID.</summary>
    public int?      LocationId      { get; init; }

    /// <summary>Filter by hiring manager ID.</summary>
    public string?   HiringManagerId { get; init; }

    /// <summary>Filter by opened/closed status.</summary>
    public bool?     IsOpend         { get; init; }

    /// <summary>Page number (1-based). Defaults to 1.</summary>
    public int       Page            { get; init; } = 1;

    /// <summary>Number of items per page. Defaults to 10.</summary>
    public int       PageSize        { get; init; } = 10;
}

/// <summary>Query parameters for searching/filtering applicants (not hired candidates) with pagination.</summary>
public class GetApplicantsQueryDto
{
    /// <summary>Filter by candidate name (partial match).</summary>
    public string? CandidateName { get; init; }

    /// <summary>Filter by job position / title (partial match).</summary>
    public string? JobPosition { get; init; }

    /// <summary>Filter by category ID.</summary>
    public int? CategoryId { get; init; }

    /// <summary>Filter by category name (partial match).</summary>
    public string? CategoryName { get; init; }

    /// <summary>Filter by status ID.</summary>
    public int? StatusId { get; init; }

    /// <summary>Page number (1-based). Defaults to 1.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Number of items per page. Defaults to 10.</summary>
    public int PageSize { get; init; } = 10;
}

// ── Offers ────────────────────────────────────────────────────────────────────

/// <summary>Full offer details — used for list rows and offer preview.</summary>
public record OfferDto(
    int       Id,
    int       CandidateId,
    string    CandidateName,
    string    CandidateCode,
    int       JobId,
    string    JobName,
    int       CategoryId,
    string    DepartmentName,
    string?   EmploymentType,
    string?   WorkLocation,
    string    OfferStatus,          // "Pending" | "Accepted" | "Declined" | "Expired"
    int       OfferStatusId,
    decimal   ProposedSalary,
    DateTime? StartDate,
    DateTime  OfferDate,
    DateTime  ExpiryDate,
    string?   Benefits,
    string?   ProfileImage,
    string?   ReasonOfRejected,
    DateTime  CreatedAt,
    string?   CreatedBy
);

/// <summary>Request body for creating a new offer.</summary>
public record CreateOfferDto
{
    [Required] public int       CandidateId    { get; init; }
    [Required] public decimal   ProposedSalary { get; init; }
    [Required] public DateTime  ExpiryDate     { get; init; }
               public DateTime? StartDate      { get; init; }
    [MaxLength(2000)] public string? Benefits  { get; init; }
}

/// <summary>Sends an offer document by email and creates a pending offer record.</summary>
public record SendOfferDto
{
    [Required] public int CandidateId { get; init; }
    [Required] public IFormFile OfferFile { get; init; } = null!;
    [MaxLength(200)] public string? Subject { get; init; }
    [MaxLength(4000)] public string? Message { get; init; }
    public decimal? ProposedSalary { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public DateTime? StartDate { get; init; }
    [MaxLength(2000)] public string? Benefits { get; init; }
}

/// <summary>Request body for updating an existing offer's details.</summary>
public record UpdateOfferDto
{
    [Required] public decimal   ProposedSalary { get; init; }
    [Required] public DateTime  ExpiryDate     { get; init; }
               public DateTime? StartDate      { get; init; }
    [MaxLength(2000)] public string? Benefits  { get; init; }
}

/// <summary>Request body for changing an offer's lifecycle status.</summary>
public record UpdateOfferStatusDto
{
    /// <summary>1=Pending, 2=Accepted, 3=Declined, 4=Expired.</summary>
    [Range((int)OfferStatus.Pending, (int)OfferStatus.Expired)]
    public int OfferStatusId { get; init; }

    /// <summary>Required when OfferStatusId is Declined (3).</summary>
    [MaxLength(2000)]
    public string? ReasonOfRejected { get; init; }
}

/// <summary>Query parameters for searching/filtering offers with pagination.</summary>
public class GetOffersQueryDto
{
    /// <summary>Filter by candidate name (partial match).</summary>
    public string?   CandidateName  { get; init; }

    /// <summary>Filter by offer status (1=Pending, 2=Accepted, 3=Declined, 4=Expired).</summary>
    public int?      OfferStatusId  { get; init; }

    /// <summary>Filter by department/category ID.</summary>
    public int?      CategoryId     { get; init; }

    /// <summary>Filter by job position ID.</summary>
    public int?      JobId          { get; init; }

    /// <summary>Filter by offer date — from (inclusive).</summary>
    public DateTime? DateFrom       { get; init; }

    /// <summary>Filter by offer date — to (inclusive).</summary>
    public DateTime? DateTo         { get; init; }

    /// <summary>Page number (1-based). Defaults to 1.</summary>
    public int       Page           { get; init; } = 1;

    /// <summary>Number of items per page. Defaults to 10.</summary>
    public int       PageSize       { get; init; } = 10;
}

/// <summary>Richer offer preview panel — includes candidate summary fields shown in the UI.</summary>
public record OfferPreviewDto(
    // Offer fields
    int       OfferId,
    string    OfferStatus,
    int       OfferStatusId,
    decimal   ProposedSalary,
    DateTime? StartDate,
    DateTime  OfferDate,
    DateTime  ExpiryDate,
    string?   Benefits,
    // Candidate info
    int       CandidateId,
    string    CandidateName,
    string    CandidateCode,
    // Job / position info
    int       JobId,
    string    JobName,
    string    DepartmentName,
    string?   EmploymentType,
    string?   WorkLocation,
    // Candidate summary (right panel in UI)
    string?   HrScore,
    string?   TechnicalScore,
    int?      CandidateRank      // rank among candidates for same job
);

/// <summary>Dashboard stats counts for each offer status.</summary>
public record OfferStatusSummaryDto(
    int TotalOffers,
    int Pending,
    int Accepted,
    int Declined,
    int Expired
);

// ── Candidate Pipeline / Progress ─────────────────────────────────────────────

/// <summary>
/// Represents a single step in the candidate hiring pipeline.
/// Status: "Completed" | "InProgress" | "Pending"
/// </summary>
public record CandidatePipelineStepDto(
    // Step display name, e.g. "Application", "HR Evaluation".
    string    StepName,

    // True when this step is fully done.
    bool      Completed,

    // "Completed" | "InProgress" | "Pending"
    string    Status,

    // Date the step was completed or last updated (null if not yet reached).
    DateTime? Date,

    // Optional extra detail (e.g. grade, offer status name, clearance name).
    string?   Detail
);

/// <summary>Full pipeline progress for a candidate — returned by GET /api/candidates/{id}/pipeline.</summary>
public record CandidatePipelineDto(
    int       CandidateId,
    string    CandidateName,

    // True when the candidate has been hired (all steps forced to Completed).
    bool      IsHired,

    // Ordered list of pipeline steps (Application → HR → Technical → Offer → Security Clearance → Hired).
    IEnumerable<CandidatePipelineStepDto> Steps
);

// ── User Statistics ────────────────────────────────────────────────────────
public record UserStatDto(
    int Count,
    decimal Percentage
);

public record UserStatisticsDto(
    int TotalUsers,
    UserStatDto ActiveUsers,
    UserStatDto InactiveUsers,
    UserStatDto Administrators,
    UserStatDto LockedAccounts,
    UserStatDto BlockedAccounts
);

// ── Dashboard Summary Cards ──────────────────────────────────────────────────
public record DashboardSummaryCardsDto(
    int OpenPositions,
    int JobsWithOpenPositions,
    int CandidatesPipe,
    int OpenRisks,
    int HighPriorityRisks
);

// ── CV Extracted Data ─────────────────────────────────────────────────────────
public record CvExtractedDataDto(
    int CandidateId,
    string CandidateName,
    List<string> Skills,
    List<string> Experience,
    List<string> Languages,
    List<string> Certificates,
    List<string> Education
);
