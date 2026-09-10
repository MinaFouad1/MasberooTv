namespace MassperoTVAPI.Core.Enums;

/// <summary>
/// Preferred working shift for a candidate.
/// </summary>
public enum PreferredShift
{
    /// <summary>Morning shift: 8 AM – 5 PM</summary>
    Morning  = 1,

    /// <summary>Evening shift: 5 PM – 12 AM</summary>
    Evening  = 2,

    /// <summary>Night shift: 12 AM – 8 AM</summary>
    Night    = 3,

    /// <summary>Flexible / no preference</summary>
    Flexible = 4
}
