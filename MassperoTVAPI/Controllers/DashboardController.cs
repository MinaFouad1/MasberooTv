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
    //[HttpGet("admin")]
    //[Authorize(Roles = "Admin")]
    //public async Task<ActionResult<ApiResponse<AdminDashboardDto>>> GetAdminDashboard()
    //{
    //    var candidates  = (await _uow.Candidates.GetAllAsync(null, null, null)).ToList();
    //    var jobs        = await _uow.Jobs.GetAllWithCategoryAsync();
    //    var interviews  = await _uow.Interviews.GetAllWithDetailsAsync();
    //    var totalUsers  = _userManager.Users.Count();

    //    // Candidates by status
    //    var byStatus = candidates
    //        .GroupBy(c => c.Status?.Name ?? "Unknown")
    //        .Select(g => new StatusCountDto(g.Key, g.Count()));

    //    // Candidates by security clearance
    //    var bySecurity = candidates
    //        .GroupBy(c => c.SecurityClearance?.Name ?? "Unknown")
    //        .Select(g => new StatusCountDto(g.Key, g.Count()));

    //    // Candidates by job
    //    var byJob = candidates
    //        .GroupBy(c => c.Job?.Name ?? "Unknown")
    //        .Select(g => new StatusCountDto(g.Key, g.Count()));

    //    // Process progress per phase
    //    var processProgress = (await BuildProcessProgress()).Phases;

    //    return Ok(ApiResponse<AdminDashboardDto>.SuccessResponse(new AdminDashboardDto(
    //        TotalCandidates:               candidates.Count,
    //        TotalJobs:                     jobs.Count(),
    //        TotalUsers:                    totalUsers,
    //        TotalInterviews:               interviews.Count(),
    //        CandidatesByStatus:            byStatus,
    //        CandidatesBySecurityClearance: bySecurity,
    //        CandidatesByJob:               byJob,
    //        ProcessProgress:               processProgress
    //    )));
    //}

    ///// <summary>Process dashboard showing all phases (or a single phase) with their processes, average completion, and overall progress.</summary>
    ///// <param name="phaseId">Optional phase ID to filter results to a single phase.</param>
    ///// <returns>Overall progress and list of phases with their associated process progress.</returns>
    //[HttpGet("processes")]
    //[Authorize(Roles = "Admin,HR,Client")]
    //public async Task<ActionResult<ApiResponse<ProcessDashboardDto>>> GetProcessDashboard([FromQuery] int? phaseId = null)
    //{
    //    var data = await BuildProcessProgress(phaseId);
    //    return Ok(ApiResponse<ProcessDashboardDto>.SuccessResponse(data));
    //}

    ///// <summary>Get process statistics: total, active, completed counts, average completion percentage, breakdown by status and phase.</summary>
    ///// <returns>Process statistics data.</returns>
    //[HttpGet("process-statistics")]
    //[Authorize(Roles = "Admin,HR,Client")]
    //public async Task<ActionResult<ApiResponse<ProcessStatisticsDto>>> GetProcessStatistics()
    //{
    //    var processes = (await _uow.Processes.GetAllWithDetailsAsync()).ToList();
    //    var completedProcesses = processes.Count(p =>
    //        string.Equals(p.ProcessStatus?.Name, "Completed", StringComparison.OrdinalIgnoreCase));

    //    var data = new ProcessStatisticsDto(
    //        TotalProcesses: processes.Count,
    //        ActiveProcesses: processes.Count - completedProcesses,
    //        CompletedProcesses: completedProcesses,
    //        AverageCompletion: processes.Count == 0
    //            ? 0
    //            : Math.Round(processes.Average(p => p.ProcessStatus?.Percentage ?? 0), 1),
    //        ByStatus: processes
    //            .GroupBy(p => p.ProcessStatus?.Name ?? "Unknown")
    //            .Select(g => new StatusCountDto(g.Key, g.Count())),
    //        ByPhase: processes
    //            .GroupBy(p => p.Phase?.Name ?? "Unknown")
    //            .Select(g => new StatusCountDto(g.Key, g.Count()))
    //    );

    //    return Ok(ApiResponse<ProcessStatisticsDto>.SuccessResponse(data));
    //}

    ///// <summary>Get issue statistics: total, resolved, unresolved counts, breakdown by resolved status, process, and priority.</summary>
    ///// <returns>Issue statistics data.</returns>
    //[HttpGet("issue-statistics")]
    //[Authorize(Roles = "Admin,HR,Client")]
    //public async Task<ActionResult<ApiResponse<IssueStatisticsDto>>> GetIssueStatistics()
    //{
    //    var issues = (await _uow.Issues.GetAllWithDetailsAsync()).ToList();
    //    var resolvedIssues   = issues.Count(i => i.Status == IssueStatus.Solved);
    //    var unresolvedIssues = issues.Count - resolvedIssues;

    //    var data = new IssueStatisticsDto(
    //        TotalIssues:      issues.Count,
    //        SolvedIssues:   resolvedIssues,
    //        OpenIssues: unresolvedIssues,
    //        ByStatus: issues
    //            .GroupBy(i => i.Status == IssueStatus.Solved ? "Resolved" : "Unresolved")
    //            .Select(g => new StatusCountDto(g.Key, g.Count())),
    //        ByProcess: issues
    //            .GroupBy(i => i.Process?.Name ?? "Unknown")
    //            .Select(g => new StatusCountDto(g.Key, g.Count())),
    //        ByPriority: issues
    //            .GroupBy(i => i.Priority.HasValue ? i.Priority.Value.ToString() : "None")
    //            .OrderBy(g => g.Key)
    //            .Select(g => new StatusCountDto(g.Key, g.Count()))
    //    );

    //    return Ok(ApiResponse<IssueStatisticsDto>.SuccessResponse(data));
    //}

    ///// <summary>Get phase statistics: total phases, each phase with its process progress.</summary>
    ///// <returns>Phase statistics data.</returns>
    //[HttpGet("phase-statistics")]
    //[Authorize(Roles = "Admin,HR,Client")]
    //public async Task<ActionResult<ApiResponse<PhaseStatisticsDto>>> GetPhaseStatistics()
    //{
    //    var dashboard = await BuildProcessProgress();
    //    var data = new PhaseStatisticsDto(
    //        TotalPhases: (await _uow.Phases.GetAllAsync()).Count(),
    //        Phases: dashboard.Phases
    //    );

    //    return Ok(ApiResponse<PhaseStatisticsDto>.SuccessResponse(data));
    //}

    /// <summary>Get phase summary with start and end dates and average completion per phase.</summary>
    /// <returns>Phases summary data.</returns>
    [HttpGet("phases-summary")]
    [Authorize(Roles = "Admin,HR,Client")]
    public async Task<ActionResult<ApiResponse<PhasesSummaryDto>>> GetPhasesSummary()
    {
        var processes = (await _uow.Processes.GetAllWithDetailsAsync()).ToList();
        var phases = (await _uow.Phases.GetAllAsync()).ToList();

        var phaseItems = phases.Select(ph =>
        {
            var phaseProcesses = processes.Where(p => p.PhaseId == ph.Id).ToList();
            var totalProcesses = phaseProcesses.Count;
            var avgComp = totalProcesses > 0
                ? Math.Round(phaseProcesses.Average(p => p.ProcessStatus?.Percentage ?? 0m), 1)
                : 0m;
            var startDate = phaseProcesses.Any() ? phaseProcesses.Min(p => (DateTime?)p.StartDate) : null;
            var endDate = phaseProcesses.Any(p => p.EndDate.HasValue) ? phaseProcesses.Max(p => p.EndDate) : null;

            return new PhaseSummaryItemDto(
                Id: ph.Id,
                Name: ph.Name,
                AverageCompletion: avgComp,
                StartDate: startDate,
                EndDate: endDate,
                TotalProcesses: totalProcesses
            );
        }).ToList();

        var data = new PhasesSummaryDto(
            TotalPhases: phases.Count,
            Phases: phaseItems
        );

        return Ok(ApiResponse<PhasesSummaryDto>.SuccessResponse(data));
    }

    /// <summary>Recruitment funnel: counts of candidates at each stage (total, HR interview, technical interview, offers sent, accepted, hired).</summary>
    /// <returns>Recruitment funnel counts.</returns>
    [HttpGet("recruitment-funnel")]
    [Authorize(Roles = "Admin,HR,Client")]
    public async Task<ActionResult<ApiResponse<RecruitmentFunnelDto>>> GetRecruitmentFunnel()
    {
        var candidates = (await _uow.Candidates.GetAllWithInterviewsAsync()).ToList();

        var candidatesCount = candidates.Count(c =>
            !string.Equals(c.Status?.Name, "Hired", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(c.Status?.Name, "SignContract", StringComparison.OrdinalIgnoreCase));

        var hrInterviewCount = candidates.Count(c =>
            c.Interviews.Any(i =>
                string.Equals(i.Type?.Name, "HR", StringComparison.OrdinalIgnoreCase)));

        var technicalInterviewCount = candidates.Count(c =>
            c.Interviews.Any(i =>
                string.Equals(i.Type?.Name, "Technical", StringComparison.OrdinalIgnoreCase)));

        var offersSent = candidates.Count(c =>
            string.Equals(c.Status?.Name, "Offered", StringComparison.OrdinalIgnoreCase));

        var offersAccepted = candidates.Count(c =>
            string.Equals(c.Status?.Name, "In Process", StringComparison.OrdinalIgnoreCase));

        var hired = candidates.Count(c =>
            string.Equals(c.Status?.Name, "Hired", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(c.Status?.Name, "SignContract", StringComparison.OrdinalIgnoreCase));

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
            Applications: j.Candidates.Count(c =>
                !string.Equals(c.Status?.Name, "Hired", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(c.Status?.Name, "SignContract", StringComparison.OrdinalIgnoreCase))
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
        [FromQuery] IssueStatus? status)
    {
        var issues = (await _uow.Issues.GetAllWithDetailsAsync()).ToList();

        if (priority.HasValue)
            issues = issues.Where(i => i.Priority == priority.Value).ToList();

        if (status.HasValue)
            issues = issues.Where(i => i.Status == status.Value).ToList();

        var result = issues
            .OrderByDescending(i => i.Priority)
            .Select(i => new RiskIssueDto(
                Id:        i.Id,
                IssueName: i.Name,
                Priority:  i.Priority,
                Status:  i.Status,
                DueDate: i.DueDate
            ));

        return Ok(ApiResponse<IEnumerable<RiskIssueDto>>.SuccessResponse(result));
    }

    /// <summary>Summary cards: Open Positions, Candidates Pipe, Open Risks.</summary>
    /// <returns>Dashboard summary metrics.</returns>
    [HttpGet("summary-cards")]
    [Authorize(Roles = "Admin,HR,Client")]
    public async Task<ActionResult<ApiResponse<DashboardSummaryCardsDto>>> GetSummaryCards()
    {
        var jobs = await _uow.Jobs.GetAllWithCategoryAsync();
        int openPositions = jobs.Where(j => j.OpenPositions > 0).Sum(j => j.OpenPositions);
        int jobsWithOpenPositions = jobs.Count(j => j.OpenPositions > 0);

        var candidates = (await _uow.Candidates.GetAllAsync(null, null, null)).ToList();
        int candidatesPipe = candidates.Count(c => 
            string.Equals(c.Status?.Name, "Under Vetting", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(c.Status?.Name, "In Process", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(c.Status?.Name, "Process", StringComparison.OrdinalIgnoreCase));

        var issues = (await _uow.Issues.GetAllWithDetailsAsync()).ToList();
        var openIssues = issues.Where(i => i.Status != IssueStatus.Solved).ToList();
        int openRisks = openIssues.Count;
        int highPriorityRisks = openIssues.Count(i => i.Priority == IssuePriority.High);

        var data = new DashboardSummaryCardsDto(
            OpenPositions: openPositions,
            JobsWithOpenPositions: jobsWithOpenPositions,
            CandidatesPipe: candidatesPipe,
            OpenRisks: openRisks,
            HighPriorityRisks: highPriorityRisks
        );

        return Ok(ApiResponse<DashboardSummaryCardsDto>.SuccessResponse(data));
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
