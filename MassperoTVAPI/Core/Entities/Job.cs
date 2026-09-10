namespace MassperoTVAPI.Core.Entities;

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
