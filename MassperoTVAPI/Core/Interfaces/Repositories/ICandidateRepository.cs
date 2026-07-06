using MassperoTVAPI.Core.Entities;

namespace MassperoTVAPI.Core.Interfaces.Repositories;

public interface ICandidateRepository : IGenericRepository<Candidate>
{
    /// <summary>Get all candidates with optional filters. Includes Job, Status, SecurityClearance, User.</summary>
    Task<IEnumerable<Candidate>> GetAllAsync(string? name, int? statusId, int? jobId);

    /// <summary>
    /// Get a paged + filtered list of candidates.
    /// Includes Job (with Category), Status, SecurityClearance, User, and Interviews (with Type).
    /// </summary>
    Task<(IEnumerable<Candidate> Items, int TotalCount)> GetPagedAsync(
        string?   candidateName,
        int?      jobId,
        int?      categoryId,
        int?      statusId,
        DateTime? dateFrom,
        DateTime? dateTo,
        int       page,
        int       pageSize);

    /// <summary>Get a candidate by ID with all related entities included.</summary>
    Task<Candidate?> GetByIdWithDetailsAsync(int id);

    /// <summary>Get all candidates including their Interviews and InterviewType for funnel analytics.</summary>
    Task<IEnumerable<Candidate>> GetAllWithInterviewsAsync();
}
