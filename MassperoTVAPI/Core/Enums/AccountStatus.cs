namespace MassperoTVAPI.Core.Enums;

/// <summary>
/// Represents the account status of an application user.
/// </summary>
public enum AccountStatus
{
    /// <summary>Account is active and can log in.</summary>
    Active = 0,

    /// <summary>Account has been manually blocked by an administrator.</summary>
    Blocked = 1,

    /// <summary>Account has been automatically locked after too many failed login attempts.</summary>
    Locked = 2
}
