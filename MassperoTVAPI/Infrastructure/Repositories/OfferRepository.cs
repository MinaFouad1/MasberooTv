using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Enums;
using MassperoTVAPI.Core.Interfaces.Repositories;
using MassperoTVAPI.Infrastructure.Data;
using MassperoTV.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MassperoTVAPI.Infrastructure.Repositories;

public class OfferRepository : GenericRepository<Offer>, IOfferRepository
{
    public OfferRepository(ApplicationDbContext context) : base(context) { }

    // ── Paged list ────────────────────────────────────────────────────────────
    public async Task<(IEnumerable<Offer> Items, int TotalCount)> GetPagedAsync(
        string?   candidateName,
        int?      offerStatusId,
        int?      categoryId,
        int?      jobId,
        DateTime? dateFrom,
        DateTime? dateTo,
        int       page,
        int       pageSize)
    {
        var query = _context.Offers
            .Include(o => o.Candidate)
                .ThenInclude(c => c.Job)
                    .ThenInclude(j => j.Category)
            .Include(o => o.Candidate)
                .ThenInclude(c => c.Job)
                    .ThenInclude(j => j.Location)
            .Include(o => o.Candidate)
                .ThenInclude(c => c.Interviews)
                    .ThenInclude(i => i.Type)
            .AsNoTracking()
            .AsQueryable();

        // ── Filters ───────────────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(candidateName))
            query = query.Where(o => o.Candidate.Name.Contains(candidateName));

        if (offerStatusId.HasValue)
            query = query.Where(o => (int)o.OfferStatusId == offerStatusId.Value);

        if (categoryId.HasValue)
            query = query.Where(o => o.Candidate.Job.CategoryId == categoryId.Value);

        if (jobId.HasValue)
            query = query.Where(o => o.Candidate.JobId == jobId.Value);

        if (dateFrom.HasValue)
            query = query.Where(o => o.OfferDate.Date >= dateFrom.Value.Date);

        if (dateTo.HasValue)
            query = query.Where(o => o.OfferDate.Date <= dateTo.Value.Date);

        // ── Pagination ────────────────────────────────────────────────────────
        var totalCount   = await query.CountAsync();
        var safePage     = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var items = await query
            .OrderByDescending(o => o.OfferDate)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    // ── Detail (for preview panel) ────────────────────────────────────────────
    public async Task<Offer?> GetByIdWithDetailsAsync(int id)
        => await _context.Offers
            .Include(o => o.Candidate)
                .ThenInclude(c => c.Job)
                    .ThenInclude(j => j.Category)
            .Include(o => o.Candidate)
                .ThenInclude(c => c.Job)
                    .ThenInclude(j => j.Location)
            .Include(o => o.Candidate)
                .ThenInclude(c => c.Interviews)
                    .ThenInclude(i => i.Type)
            .FirstOrDefaultAsync(o => o.Id == id);

    // ── Status summary (dashboard cards) ─────────────────────────────────────
    public async Task<OfferStatusSummaryDto> GetStatusSummaryAsync()
    {
        var groups = await _context.Offers
            .GroupBy(o => o.OfferStatusId)
            .Select(g => new { StatusId = (int)g.Key, Count = g.Count() })
            .ToListAsync();

        int Get(OfferStatus s) => groups.FirstOrDefault(g => g.StatusId == (int)s)?.Count ?? 0;

        return new OfferStatusSummaryDto(
            TotalOffers: groups.Sum(g => g.Count),
            Pending:     Get(OfferStatus.Pending),
            Accepted:    Get(OfferStatus.Accepted),
            Declined:    Get(OfferStatus.Declined),
            Expired:     Get(OfferStatus.Expired)
        );
    }

    // ── Candidate rank within the same job ────────────────────────────────────
    public async Task<int> GetCandidateRankAsync(int candidateId, int jobId)
    {
        // Rank = position of candidateId when candidates for jobId are sorted by Id asc
        var ids = await _context.Candidates
            .Where(c => c.JobId == jobId)
            .OrderBy(c => c.Id)
            .Select(c => c.Id)
            .ToListAsync();

        var idx = ids.IndexOf(candidateId);
        return idx < 0 ? 0 : idx + 1;
    }
}
