using MassperoTVAPI.Modules.Hr.Domain.Entities.Categories;
using MassperoTVAPI.Modules.Hr.Domain.Entities.Lookups;
using MassperoTVAPI.Modules.Hr.Domain.Entities.Identity;
using MassperoTVAPI.Modules.Hr.Domain.Entities.Candidates;

namespace MassperoTVAPI.Modules.Hr.Domain.Entities.Jobs;

public class Job
{
    public int     Id             { get; set; }
    public string  Name           { get; set; } = string.Empty;
    public string? Desc           { get; set; }
    public bool IsOpend { get; set; } = true;

    public int       OpenPositions    { get; set; } = 0;
    public DateTime  CreatedAt        { get; set; } = DateTime.UtcNow;
    public string?   EmploymentType   { get; set; }
    public DateTime? TargetHiringDate { get; set; }
    public DateTime? DeadLineDate { get; set; }

    public string? Responsibilities { get; set; }
    public string? RequiredSkills { get; set; }
    public string? ExperienceLevel { get; set; }
    public string? Education { get; set; }
    public string? Languages { get; set; }


    // FK → Category
    public int      CategoryId { get; set; }
    public Category Category   { get; set; } = null!;

    // FK → Location (optional)
    public int?      LocationId { get; set; }
    public Location? Location   { get; set; }

    // FK → HiringManager (ApplicationUser, optional)
    public string?          HiringManagerId { get; set; }
    public ApplicationUser? HiringManager   { get; set; }

    // Navigation
    public ICollection<Candidate> Candidates { get; set; } = [];
}
