namespace MassperoTVAPI.Core.Entities;

public class InterviewType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation
    public ICollection<Interview> Interviews { get; set; } = [];
}
