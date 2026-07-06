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
public record CategoryDto(int Id, string Name);
public record CreateCategoryDto
{
    [Required] [MaxLength(150)] public string Name { get; init; } = string.Empty;
}
public record UpdateCategoryDto
{
    [Required] [MaxLength(150)] public string Name { get; init; } = string.Empty;
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
    int      Id,
    string   Name,
    string?  Desc,
    int      CategoryId,
    string   CategoryName,
    int      OpenPositions,
    DateTime CreatedAt,
    string?  EmploymentType,
    DateTime? TargetHiringDate,
    int?     LocationId,
    string?  LocationName,
    int      Applicants,
    int      Interviews
);
public record CreateJobDto
{
    [Required] [MaxLength(200)] public string    Name             { get; init; } = string.Empty;
    [MaxLength(1000)]           public string?   Desc             { get; init; }
    [Required]                  public int       CategoryId       { get; init; }
                                public int       OpenPositions    { get; init; } = 0;
    [MaxLength(100)]            public string?   EmploymentType   { get; init; }
                                public DateTime? TargetHiringDate { get; init; }
                                public int?      LocationId       { get; init; }
}
public record UpdateJobDto
{
    [Required] [MaxLength(200)] public string    Name             { get; init; } = string.Empty;
    [MaxLength(1000)]           public string?   Desc             { get; init; }
    [Required]                  public int       CategoryId       { get; init; }
                                public int       OpenPositions    { get; init; } = 0;
    [MaxLength(100)]            public string?   EmploymentType   { get; init; }
                                public DateTime? TargetHiringDate { get; init; }
                                public int?      LocationId       { get; init; }
}

// ── Candidate ─────────────────────────────────────────────────────────────────
public record CandidateDto(
    int     Id,
    string  Name,
    string? CvFile,
    string? ReasonOfAccept,
    string? ReasonOfReject,
    bool?    Accepted,
    int     JobId,
    string  JobName,
    int     StatusId,
    string  StatusName,
    int     SecurityClearanceId,
    string  SecurityClearanceName,
    string? ApplicationUserId,
    string? ApplicationUserName
);
public record CreateCandidateDto
{
    [Required] [MaxLength(200)] public string  Name                { get; init; } = string.Empty;
    public IFormFile?                         CvFile              { get; init; }
    [Required]                  public int     JobId               { get; init; }

}
public record UpdateCandidateDto
{
    [Required] [MaxLength(200)] public string  Name                { get; init; } = string.Empty;
    public IFormFile?                         CvFile              { get; init; }
    [MaxLength(1000)]           public string? ReasonOfAccept      { get; init; }
    [MaxLength(1000)]           public string? ReasonOfReject      { get; init; }
    [Required]                  public int     JobId               { get; init; }
}

// ── Interview ─────────────────────────────────────────────────────────────────
public record InterviewDto(
    int     Id,
    string? Grade,
    string? Comments,
    int     CandidateId,
    string  CandidateName,
    int     TypeId,
    string  TypeName
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

// ── Issue ─────────────────────────────────────────────────────────────────────
public record IssueDetailDto(
    int Id,
    string Name,
    bool? Resolved,
    DateTime? Date,
    IssuePriority? Priority,
    int ProcessId,
    string ProcessName
);
public record CreateIssueDto
{
    [Required] [MaxLength(300)] public string         Name      { get; init; } = string.Empty;
    [Required]                  public int            ProcessId { get; init; }
    public bool?          Resolved  { get; init; }
    public DateTime?      Date      { get; init; }
    public IssuePriority? Priority  { get; init; }
}
public record UpdateIssueDto
{
    [Required] [MaxLength(300)] public string         Name      { get; init; } = string.Empty;
    [Required]                  public int            ProcessId { get; init; }
    public bool?          Resolved  { get; init; }
    public DateTime?      Date      { get; init; }
    public IssuePriority? Priority  { get; init; }
}
public record PatchIssueResolvedDto
{
    [Required] public bool Resolved { get; init; }
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
    string Email,
    bool   IsVerified,
    IList<string> Roles
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
    int ResolvedIssues,
    int UnresolvedIssues,
    IEnumerable<StatusCountDto> ByResolvedStatus,
    IEnumerable<StatusCountDto> ByProcess,
    IEnumerable<StatusCountDto> ByPriority
);

public record PhaseStatisticsDto(
    int TotalPhases,
    IEnumerable<ProcessPhaseProgressDto> Phases
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
    bool?          Resolved
);

// ── Job Statistics ────────────────────────────────────────────────────────────
public record JobStatisticsDto(
    int TotalJobs,
    int OpenPositions,
    int Applications,
    int OffersSent
);

// ── Interview grade patch ─────────────────────────────────────────────────────
/// <summary>PATCH body to update the grade (and optionally comments) of an interview.</summary>
public record PatchInterviewGradeDto
{
    [Required] [MaxLength(50)]   public string  Grade    { get; init; } = string.Empty;
    [MaxLength(2000)]            public string? Comments { get; init; }
}
