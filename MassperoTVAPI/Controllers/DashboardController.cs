using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Enums;
using MassperoTVAPI.Core.Helpers;
using MassperoTVAPI.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MassperoTVAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IUnitOfWork                  _uow;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
    {
        _uow         = uow;
        _userManager = userManager; 
    }

    /// <summary>Admin dashboard with aggregated statistics: total candidates, jobs, users, interviews, and breakdowns by status, security clearance, job, and process progress.</summary>
    /// <returns>Comprehensive admin dashboard data.</returns>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<AdminDashboardDto>>> GetAdminDashboard()
    {
        var candidates  = (await _uow.Candidates.GetAllAsync(null, null, null)).ToList();
        var jobs        = await _uow.Jobs.GetAllWithCategoryAsync();
        var interviews  = await _uow.Interviews.GetAllWithDetailsAsync();
        var totalUsers  = _userManager.Users.Count();

        // Candidates by status
        var byStatus = candidates
            .GroupBy(c => c.Status?.Name ?? "Unknown")
            .Select(g => new StatusCountDto(g.Key, g.Count()));

        // Candidates by security clearance
        var bySecurity = candidates
            .GroupBy(c => c.SecurityClearance?.Name ?? "Unknown")
            .Select(g => new StatusCountDto(g.Key, g.Count()));

        // Candidates by job
        var byJob = candidates
            .GroupBy(c => c.Job?.Name ?? "Unknown")
            .Select(g => new StatusCountDto(g.Key, g.Count()));

        // Process progress per phase
        var processProgress = (await BuildProcessProgress()).Phases;

        return Ok(ApiResponse<AdminDashboardDto>.SuccessResponse(new AdminDashboardDto(
            TotalCandidates:               candidates.Count,
            TotalJobs:                     jobs.Count(),
            TotalUsers:                    totalUsers,
            TotalInterviews:               interviews.Count(),
            CandidatesByStatus:            byStatus,
            CandidatesBySecurityClearance: bySecurity,
            CandidatesByJob:               byJob,
            ProcessProgress:               processProgress
        )));
    }

    /// <summary>Process dashboard showing all phases (or a single phase) with their processes, average completion, and overall progress.</summary>
    /// <param name="phaseId">Optional phase ID to filter results to a single phase.</param>
    /// <returns>Overall progress and list of phases with their associated process progress.</returns>
    [HttpGet("processes")]
    [Authorize(Roles = "Admin,HR,Client")]
    public async Task<ActionResult<ApiResponse<ProcessDashboardDto>>> GetProcessDashboard([FromQuery] int? phaseId = null)
    {
        var data = await BuildProcessProgress(phaseId);
        return Ok(ApiResponse<ProcessDashboardDto>.SuccessResponse(data));
    }

    /// <summary>Get process statistics: total, active, completed counts, average completion percentage, breakdown by status and phase.</summary>
    /// <returns>Process statistics data.</returns>
    [HttpGet("process-statistics")]
    [Authorize(Roles = "Admin,HR,Client")]
    public async Task<ActionResult<ApiResponse<ProcessStatisticsDto>>> GetProcessStatistics()
    {
        var processes = (await _uow.Processes.GetAllWithDetailsAsync()).ToList();
        var completedProcesses = processes.Count(p =>
            string.Equals(p.ProcessStatus?.Name, "Completed", StringComparison.OrdinalIgnoreCase));

        var data = new ProcessStatisticsDto(
            TotalProcesses: processes.Count,
            ActiveProcesses: processes.Count - completedProcesses,
            CompletedProcesses: completedProcesses,
            AverageCompletion: processes.Count == 0
                ? 0
                : Math.Round(processes.Average(p => p.ProcessStatus?.Percentage ?? 0), 1),
            ByStatus: processes
                .GroupBy(p => p.ProcessStatus?.Name ?? "Unknown")
                .Select(g => new StatusCountDto(g.Key, g.Count())),
            ByPhase: processes
                .GroupBy(p => p.Phase?.Name ?? "Unknown")
                .Select(g => new StatusCountDto(g.Key, g.Count()))
        );

        return Ok(ApiResponse<ProcessStatisticsDto>.SuccessResponse(data));
    }

    /// <summary>Get issue statistics: total, resolved, unresolved counts, breakdown by resolved status, process, and priority.</summary>
    /// <returns>Issue statistics data.</returns>
    [HttpGet("issue-statistics")]
    [Authorize(Roles = "Admin,HR,Client")]
    public async Task<ActionResult<ApiResponse<IssueStatisticsDto>>> GetIssueStatistics()
    {
        var issues = (await _uow.Issues.GetAllWithDetailsAsync()).ToList();
        var resolvedIssues   = issues.Count(i => i.Resolved == true);
        var unresolvedIssues = issues.Count - resolvedIssues;

        var data = new IssueStatisticsDto(
            TotalIssues:      issues.Count,
            ResolvedIssues:   resolvedIssues,
            UnresolvedIssues: unresolvedIssues,
            ByResolvedStatus: issues
                .GroupBy(i => i.Resolved == true ? "Resolved" : "Unresolved")
                .Select(g => new StatusCountDto(g.Key, g.Count())),
            ByProcess: issues
                .GroupBy(i => i.Process?.Name ?? "Unknown")
                .Select(g => new StatusCountDto(g.Key, g.Count())),
            ByPriority: issues
                .GroupBy(i => i.Priority.HasValue ? i.Priority.Value.ToString() : "None")
                .OrderBy(g => g.Key)
                .Select(g => new StatusCountDto(g.Key, g.Count()))
        );

        return Ok(ApiResponse<IssueStatisticsDto>.SuccessResponse(data));
    }

    /// <summary>Get phase statistics: total phases, each phase with its process progress.</summary>
    /// <returns>Phase statistics data.</returns>
    [HttpGet("phase-statistics")]
    [Authorize(Roles = "Admin,HR,Client")]
    public async Task<ActionResult<ApiResponse<PhaseStatisticsDto>>> GetPhaseStatistics()
    {
        var dashboard = await BuildProcessProgress();
        var data = new PhaseStatisticsDto(
            TotalPhases: (await _uow.Phases.GetAllAsync()).Count(),
            Phases: dashboard.Phases
        );

        return Ok(ApiResponse<PhaseStatisticsDto>.SuccessResponse(data));
    }

    /// <summary>Recruitment funnel: counts of candidates at each stage (total, HR interview, technical interview, offers sent, accepted, hired).</summary>
    /// <returns>Recruitment funnel counts.</returns>
    [HttpGet("recruitment-funnel")]
    [Authorize(Roles = "Admin,HR,Client")]
    public async Task<ActionResult<ApiResponse<RecruitmentFunnelDto>>> GetRecruitmentFunnel()
    {
        var candidates = (await _uow.Candidates.GetAllWithInterviewsAsync()).ToList();

        var candidatesCount = candidates.Count;

        var hrInterviewCount = candidates.Count(c =>
            c.Interviews.Any(i =>
                string.Equals(i.Type?.Name, "HR", StringComparison.OrdinalIgnoreCase)));

        var technicalInterviewCount = candidates.Count(c =>
            c.Interviews.Any(i =>
                string.Equals(i.Type?.Name, "Technical", StringComparison.OrdinalIgnoreCase)));

        var offersSent = candidates.Count(c =>
            string.Equals(c.Status?.Name, "Offered", StringComparison.OrdinalIgnoreCase));

        var offersAccepted = candidates.Count(c =>
            string.Equals(c.Status?.Name, "Contracted", StringComparison.OrdinalIgnoreCase));

        var hired = candidates.Count(c =>
            string.Equals(c.Status?.Name, "In Production", StringComparison.OrdinalIgnoreCase));

        return Ok(ApiResponse<RecruitmentFunnelDto>.SuccessResponse(new RecruitmentFunnelDto(
            CandidatesCount:        candidatesCount,
            HrInterviewCount:       hrInterviewCount,
            TechnicalInterviewCount: technicalInterviewCount,
            OffersSent:             offersSent,
            OffersAccepted:         offersAccepted,
            Hired:                  hired
        )));
    }

    /// <summary>Job status: each job with its title, number of open positions, and count of applicants.</summary>
    /// <returns>List of job status entries.</returns>
    [HttpGet("job-status")]
    [Authorize(Roles = "Admin,HR,Client")]
    public async Task<ActionResult<ApiResponse<IEnumerable<JobStatusDto>>>> GetJobStatus()
    {
        var jobs = (await _uow.Jobs.GetAllWithCandidatesAsync()).ToList();

        var result = jobs.Select(j => new JobStatusDto(
            JobTitle:      j.Name,
            OpenPositions: j.OpenPositions,
            Applications:  j.Candidates.Count
        ));

        return Ok(ApiResponse<IEnumerable<JobStatusDto>>.SuccessResponse(result));
    }

    /// <summary>Risk management issues: list of all issues with their name, priority, and resolved status.</summary>
    /// <param name="priority">Optional. Filter by priority level (1=Low, 2=Medium, 3=High, 4=Critical).</param>
    /// <param name="resolved">Optional. Filter by resolved status.</param>
    /// <returns>List of risk issues ordered by priority descending (Critical first).</returns>
    [HttpGet("risk-issues")]
    [Authorize(Roles = "Admin,HR,Client")]
    public async Task<ActionResult<ApiResponse<IEnumerable<RiskIssueDto>>>> GetRiskIssues(
        [FromQuery] IssuePriority? priority,
        [FromQuery] bool? resolved)
    {
        var issues = (await _uow.Issues.GetAllWithDetailsAsync()).ToList();

        if (priority.HasValue)
            issues = issues.Where(i => i.Priority == priority.Value).ToList();

        if (resolved.HasValue)
            issues = issues.Where(i => i.Resolved == resolved.Value).ToList();

        var result = issues
            .OrderByDescending(i => i.Priority)
            .Select(i => new RiskIssueDto(
                Id:        i.Id,
                IssueName: i.Name,
                Priority:  i.Priority,
                Resolved:  i.Resolved
            ));

        return Ok(ApiResponse<IEnumerable<RiskIssueDto>>.SuccessResponse(result));
    }

    // ── Private helper ────────────────────────────────────────────────────────
    private async Task<ProcessDashboardDto> BuildProcessProgress(int? phaseId = null)
    {
        var processes = (await _uow.Processes.GetAllWithDetailsAsync()).ToList();

        if (phaseId.HasValue)
            processes = processes.Where(p => p.PhaseId == phaseId.Value).ToList();

        var overallProgress = processes.Count == 0
            ? 0
            : Math.Round(processes.Average(p => p.ProcessStatus?.Percentage ?? 0), 1);

        var phases = processes
            .GroupBy(p => new { p.PhaseId, PhaseName = p.Phase?.Name ?? "Unknown" })
            .Select(g =>
            {
                var summaries = g.Select(p => new ProcessSummaryDto(
                    Id:              p.Id,
                    Name:            p.Name,
                    StatusName:      p.ProcessStatus?.Name       ?? string.Empty,
                    Percentage:      p.ProcessStatus?.Percentage ?? 0,
                    StartDate:       p.StartDate,
                    EndDate:         p.EndDate,
                    IssueCount:      p.Issues?.Count      ?? 0,
                    DependencyCount: p.Dependencies?.Count ?? 0
                )).ToList();

                return new ProcessPhaseProgressDto(
                    PhaseId:           g.Key.PhaseId,
                    PhaseName:         g.Key.PhaseName,
                    TotalProcesses:    summaries.Count,
                    AverageCompletion: summaries.Count > 0
                        ? Math.Round(summaries.Average(s => s.Percentage), 1)
                        : 0,
                    Processes: summaries
                );
            })
            .OrderBy(x => x.PhaseId)
            .ToList();

        return new ProcessDashboardDto(
            OverallProgress: overallProgress,
            Phases:          phases
        );
    }
}
