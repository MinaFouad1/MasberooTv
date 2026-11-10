namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Holidays;

/// <summary>
/// Allowed values for the existing string column Holiday.HolidayType
/// (entity default "National", entity comment "or company").
/// </summary>
public static class HolidayTypes
{
    public const string National = "National";
    public const string Company  = "Company";

    public static readonly IReadOnlyList<string> All = [National, Company];

    /// <summary>Returns the canonical value, or null when the value is not allowed.</summary>
    public static string? Normalize(string value)
        => All.FirstOrDefault(t => string.Equals(t, value.Trim(), StringComparison.OrdinalIgnoreCase));
}