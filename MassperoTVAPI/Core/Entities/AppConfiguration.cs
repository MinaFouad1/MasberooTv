namespace MassperoTVAPI.Core.Entities;

/// <summary>
/// Stores file-type configuration entries.
/// Key  = folder/type name (e.g. "Document", "Evidence", "Profile")
/// Value = base URL for that folder (e.g. "http://192.168.1.163:500/Uploads/Document/")
/// </summary>
public class AppConfiguration
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
