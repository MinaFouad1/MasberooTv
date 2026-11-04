namespace MassperoTVAPI.Core.Entities;

public class Employee
{
    public int     Id             { get; set; }
    public string  Name           { get; set; } = string.Empty;
    public string? EmploymentType { get; set; }

    // FK -> Job (optional)
    public int? JobId { get; set; }
    public Job? Job   { get; set; }

    // FK -> Team (optional)
    public int?  TeamId { get; set; }
    public Team? Team   { get; set; }

    // FK -> Candidate (optional, unique when set)
    public int?       CandidateId { get; set; }
    public Candidate? Candidate   { get; set; }
}
