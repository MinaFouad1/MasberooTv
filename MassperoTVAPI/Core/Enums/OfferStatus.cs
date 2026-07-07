namespace MassperoTVAPI.Core.Enums;

/// <summary>
/// Represents the lifecycle status of a job offer.
/// Stored as int in the database (OfferStatusId column).
/// </summary>
public enum OfferStatus
{
    Pending  = 1,
    Accepted = 2,
    Declined = 3,
    Expired  = 4
}
