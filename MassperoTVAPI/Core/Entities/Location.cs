namespace MassperoTVAPI.Core.Entities;

public class Location
{
    public int    Id   { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation
    public ICollection<Job> Jobs { get; set; } = [];
}
