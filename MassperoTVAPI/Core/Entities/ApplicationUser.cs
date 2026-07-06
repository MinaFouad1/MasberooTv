using Microsoft.AspNetCore.Identity;

namespace MassperoTVAPI.Core.Entities;

public class ApplicationUser : IdentityUser
{
    /// <summary>Whether the user's email/account has been verified.</summary>
    public bool IsVerified { get; set; }

    // Navigation Properties
    public ICollection<Candidate> Candidates { get; set; } = [];
}
