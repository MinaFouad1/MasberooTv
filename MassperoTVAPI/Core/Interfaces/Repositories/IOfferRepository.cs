using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Enums;

namespace MassperoTVAPI.Core.Interfaces.Repositories;

public interface IOfferRepository : IGenericRepository<Offer>
{
    /// <summary>
    /// Paged + filtered list of offers.
    /// Includes Candidate → Job (Category, Location), Interviews (Type).
    /// </summary>
    Task<(IEnumerable<Offer> Items, int TotalCount)> GetPagedAsync(
        string?      candidateName,
        int?         offerStatusId,
        int?         categoryId,
        int?         jobId,
        DateTime?    dateFrom,
        DateTime?    dateTo,
        int          page,
        int          pageSize);

    /// <summary>Get a single offer by ID with all related data for the preview panel.</summary>
    Task<Offer?> GetByIdWithDetailsAsync(int id);

    /// <summary>Aggregate counts per offer status (for dashboard cards).</summary>
    Task<OfferStatusSummaryDto> GetStatusSummaryAsync();

    /// <summary>
    /// Returns the rank (position) of the candidate among all candidates for the same job
    /// ordered by their Id (lower Id = earlier applicant = rank 1).
    /// </summary>
    Task<int> GetCandidateRankAsync(int candidateId, int jobId);

    /// <summary>Returns all offers for a specific candidate (lightweight, for pipeline checks).</summary>
    Task<IEnumerable<Offer>> GetByCandidateAsync(int candidateId);
}
