using MassperoTVAPI.Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace MassperoTVAPI.Core.Entities;

public class ApplicationUser : IdentityUser
{
    /// <summary>Whether the user's email/account has been verified.</summary>
    public bool IsVerified { get; set; }

    /// <summary>Current account status: Active, Blocked (admin), or Locked (auto after 3 failed attempts).</summary>
    public AccountStatus AccountStatus { get; set; } = AccountStatus.Active;

    /// <summary>Number of consecutive failed login attempts. Resets to 0 on successful login.</summary>
    public int FailedLoginAttempts { get; set; } = 0;

    /// <summary>UTC timestamp of the user's last successful login.</summary>
    public DateTime? LastLoginAt { get; set; }

    // Navigation Properties
    public ICollection<Candidate> Candidates { get; set; } = [];
    public ICollection<Interview> EvaluatedInterviews { get; set; } = [];
    public ICollection<Issue> CreatedIssues { get; set; } = [];
    public ICollection<Job>   ManagedJobs   { get; set; } = [];
}
