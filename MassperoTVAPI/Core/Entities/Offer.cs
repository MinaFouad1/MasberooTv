using MassperoTVAPI.Core.Enums;

namespace MassperoTVAPI.Core.Entities;

public class Offer
{
    public int Id { get; set; }

    // FK → Candidate
    public int CandidateId { get; set; }
    public Candidate Candidate { get; set; } = null!;

    /// <summary>Offer lifecycle status (Pending, Accepted, Declined, Expired).</summary>
    public OfferStatus OfferStatusId { get; set; } = OfferStatus.Pending;

    public decimal ProposedSalary { get; set; }

    /// <summary>Proposed employment start date (optional at creation).</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>Date the offer was issued. Defaults to now.</summary>
    public DateTime OfferDate { get; set; } = DateTime.UtcNow;

    /// <summary>Date the offer expires.</summary>
    public DateTime ExpiryDate { get; set; }

    /// <summary>Free-text benefits description (health, bonuses, etc.).</summary>
    public string? Benefits { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Username / user-id of the HR who created this offer.</summary>
    public string? CreatedBy { get; set; }
}
