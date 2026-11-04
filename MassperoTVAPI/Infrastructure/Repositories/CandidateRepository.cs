using MassperoTV.Infrastructure.Repositories;
using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Interfaces.Repositories;
using MassperoTVAPI.Core.Mappers;
using MassperoTVAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MassperoTVAPI.Modules.Hr.Domain.Entities.Candidates;

namespace MassperoTVAPI.Infrastructure.Repositories;

public class CandidateRepository : GenericRepository<Candidate>, ICandidateRepository
{
    public CandidateRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Candidate>> GetAllAsync(string? name, int? statusId, int? jobId)
    {
        var query = _context.Candidates
            .Include(c => c.Job)
            .Include(c => c.Status)
            .Include(c => c.SecurityClearance)
            .Include(c => c.User)
            .Include(c => c.CandidateSkills)
                .ThenInclude(cs => cs.Skill)
            .Include(c => c.CandidateLanguages)
                .ThenInclude(cl => cl.Language)
            .Include(c => c.Certifications)
            .Include(c => c.Educations)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(c => c.Name.Contains(name));

        if (statusId.HasValue)
            query = query.Where(c => c.StatusId == statusId.Value);

        if (jobId.HasValue)
            query = query.Where(c => c.JobId == jobId.Value);

        return await query.ToListAsync();
    }

    public async Task<(IEnumerable<Candidate> Items, int TotalCount)> GetPagedAsync(
        string?   candidateName,
        int?      jobId,
        int?      categoryId,
        int?      statusId,
        DateTime? dateFrom,
        DateTime? dateTo,
        int       page,
        int       pageSize)
    {
        var query = _context.Candidates
            .Include(c => c.Job)
                .ThenInclude(j => j.Category)
            .Include(c => c.Status)
            .Include(c => c.SecurityClearance)
            .Include(c => c.User)
            .Include(c => c.Interviews)
                .ThenInclude(i => i.Type)
            .Include(c => c.CandidateSkills)
                .ThenInclude(cs => cs.Skill)
            .Include(c => c.CandidateLanguages)
                .ThenInclude(cl => cl.Language)
            .Include(c => c.Certifications)
            .Include(c => c.Educations)
            .AsNoTracking()
            .AsQueryable();

        // ── Filters ──────────────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(candidateName))
            query = query.Where(c => c.Name.Contains(candidateName));

        if (jobId.HasValue)
            query = query.Where(c => c.JobId == jobId.Value);

        if (categoryId.HasValue)
            query = query.Where(c => c.Job.CategoryId == categoryId.Value);

        if (statusId.HasValue)
            query = query.Where(c => c.StatusId == statusId.Value);

        if (dateFrom.HasValue)
            query = query.Where(c => c.HiringDate.HasValue && c.HiringDate.Value.Date >= dateFrom.Value.Date);

        if (dateTo.HasValue)
            query = query.Where(c => c.HiringDate.HasValue && c.HiringDate.Value.Date <= dateTo.Value.Date);

        // ── Pagination ────────────────────────────────────────────────────────
        var totalCount = await query.CountAsync();

        var safePage     = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var items = await query
            .OrderByDescending(c => c.Id)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(IEnumerable<Candidate> Items, int TotalCount)> GetApplicantsAsync(GetApplicantsQueryDto queryDto)
    {
        var dbQuery = _context.Candidates
            .Include(c => c.Job)
                .ThenInclude(j => j.Category)
            .Include(c => c.Status)
            .Include(c => c.Interviews)
                .ThenInclude(i => i.Type)
            .Where(c => c.Status.Name != "Hired" && c.Status.Name != "SignContract")
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryDto.CandidateName))
            dbQuery = dbQuery.Where(c => c.Name.Contains(queryDto.CandidateName));

        if (!string.IsNullOrWhiteSpace(queryDto.JobPosition))
            dbQuery = dbQuery.Where(c => c.Job.Name.Contains(queryDto.JobPosition));

        if (queryDto.CategoryId.HasValue)
            dbQuery = dbQuery.Where(c => c.Job.CategoryId == queryDto.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(queryDto.CategoryName))
            dbQuery = dbQuery.Where(c => c.Job.Category.Name.Contains(queryDto.CategoryName));

        if (queryDto.StatusId.HasValue)
            dbQuery = dbQuery.Where(c => c.StatusId == queryDto.StatusId.Value);

        var totalCount = await dbQuery.CountAsync();

        var safePage = Math.Max(1, queryDto.PageNumber);
        var safePageSize = Math.Clamp(queryDto.PageSize, 1, 100);

        var items = await dbQuery
            .OrderByDescending(c => c.Id)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Candidate?> GetByIdWithDetailsAsync(int id)
        => await _context.Candidates
            .Include(c => c.Job)
                .ThenInclude(j => j.Category)
            .Include(c => c.Job)
                .ThenInclude(j => j.Location)
            .Include(c => c.Status)
            .Include(c => c.SecurityClearance)
            .Include(c => c.User)
            .Include(c => c.Interviews)
                .ThenInclude(i => i.Type)
            .Include(c => c.CandidateSkills)
                .ThenInclude(cs => cs.Skill)
            .Include(c => c.CandidateLanguages)
                .ThenInclude(cl => cl.Language)
            .Include(c => c.Certifications)
            .Include(c => c.Educations)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task SyncProfileCollectionsAsync(
        Candidate candidate,
        IEnumerable<int>? skillIds,
        IEnumerable<int>? languageIds,
        IEnumerable<UpsertCandidateCertificationDto>? certifications,
        IEnumerable<UpsertCandidateEducationDto>? educations)
    {
        // ── 1. CandidateSkills table ──────────────────────────────────────────
        if (skillIds is not null)
        {
            var existingSkills = await _context.CandidateSkills
                .Where(cs => cs.CandidateId == candidate.Id)
                .ToListAsync();

            if (existingSkills.Count > 0)
            {
                _context.CandidateSkills.RemoveRange(existingSkills);
                await _context.SaveChangesAsync();
            }

            var targetSkillIds = new HashSet<int>();

            foreach (var sid in skillIds.Where(id => id > 0))
            {
                if (await _context.Skills.AnyAsync(s => s.Id == sid))
                {
                    targetSkillIds.Add(sid);
                }
            }

            foreach (var sid in targetSkillIds)
            {
                await _context.CandidateSkills.AddAsync(new CandidateSkill
                {
                    CandidateId = candidate.Id,
                    SkillId = sid,
                    YearsOfExperience = 0
                });
            }
            await _context.SaveChangesAsync();
        }

        // ── 2. CandidateLanguages table (without duplicates) ──────────────────
        if (languageIds is not null)
        {
            var existingLanguages = await _context.CandidateLanguages
                .Where(cl => cl.CandidateId == candidate.Id)
                .ToListAsync();

            if (existingLanguages.Count > 0)
            {
                _context.CandidateLanguages.RemoveRange(existingLanguages);
                await _context.SaveChangesAsync();
            }

            var targetLanguageIds = new HashSet<int>();

            foreach (var lid in languageIds.Where(id => id > 0))
            {
                if (await _context.Languages.AnyAsync(l => l.Id == lid))
                {
                    targetLanguageIds.Add(lid);
                }
            }

            foreach (var langId in targetLanguageIds)
            {
                await _context.CandidateLanguages.AddAsync(new CandidateLanguage
                {
                    CandidateId = candidate.Id,
                    LanguageId = langId
                });
            }
            await _context.SaveChangesAsync();
        }

        // ── 3. Certifications table ───────────────────────────────────────────
        if (certifications is not null)
        {
            var existingCerts = await _context.Certifications
                .Where(c => c.CandidateId == candidate.Id)
                .ToListAsync();

            if (existingCerts.Count > 0)
            {
                _context.Certifications.RemoveRange(existingCerts);
                await _context.SaveChangesAsync();
            }

            var validCerts = certifications
                .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                .GroupBy(c => c.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            foreach (var item in validCerts)
            {
                await _context.Certifications.AddAsync(new Certification
                {
                    CandidateId = candidate.Id,
                    Name = item.Name.Trim(),
                    Url = string.IsNullOrWhiteSpace(item.Url) ? null : item.Url.Trim()
                });
            }
            await _context.SaveChangesAsync();
        }

        // ── 4. Educations table ───────────────────────────────────────────────
        if (educations is not null)
        {
            var existingEducations = await _context.Educations
                .Where(e => e.CandidateId == candidate.Id)
                .ToListAsync();

            if (existingEducations.Count > 0)
            {
                _context.Educations.RemoveRange(existingEducations);
                await _context.SaveChangesAsync();
            }

            var validEducations = educations
                .Where(e => !string.IsNullOrWhiteSpace(e.Degree) && !string.IsNullOrWhiteSpace(e.University))
                .GroupBy(e => (Degree: e.Degree.Trim().ToLowerInvariant(), University: e.University.Trim().ToLowerInvariant(), Year: e.GraduationYear))
                .Select(g => g.First())
                .ToList();

            foreach (var item in validEducations)
            {
                await _context.Educations.AddAsync(new Education
                {
                    CandidateId = candidate.Id,
                    Degree = item.Degree.Trim(),
                    University = item.University.Trim(),
                    GraduationYear = item.GraduationYear,
                    Grade = string.IsNullOrWhiteSpace(item.Grade) ? null : item.Grade.Trim()
                });
            }
            await _context.SaveChangesAsync();
        }

        // Detach or clear tracker to guarantee fresh re-query with all includes
        _context.ChangeTracker.Clear();
    }

    public async Task<IEnumerable<Candidate>> GetAllWithInterviewsAsync()
        => await _context.Candidates
            .Include(c => c.Status)
            .Include(c => c.Interviews)
                .ThenInclude(i => i.Type)
            .AsNoTracking()
            .ToListAsync();

    public async Task<(int Rank, int TotalCandidates)> GetRankInJobAsync(int candidateId, int jobId)
    {
        // Load all candidates for this job with their interviews
        var peers = await _context.Candidates
            .Where(c => c.JobId == jobId)
            .Include(c => c.Interviews)
            .AsNoTracking()
            .ToListAsync();

        // Compute average score for each candidate (null if no parseable grades)
        var ranked = peers
            .Select(c => new
            {
                c.Id,
                AvgScore = c.Interviews
                    .Select(i => decimal.TryParse(i.Grade, out var g) ? (decimal?)g : null)
                    .Where(g => g.HasValue)
                    .Select(g => g!.Value)
                    .DefaultIfEmpty(0m)
                    .Average()
            })
            .OrderByDescending(x => x.AvgScore)
            .ThenBy(x => x.Id)          // stable tie-breaker: earlier applicant ranks higher
            .ToList();

        var idx = ranked.FindIndex(x => x.Id == candidateId);
        return (idx < 0 ? 0 : idx + 1, ranked.Count);   // 1-based; 0 = not found
    }

    public async Task<IEnumerable<Candidate>> GetJobRankingAsync(int jobId)
    {
        // Load all candidates for this job with their interviews
        var peers = await _context.Candidates
            .Where(c => c.JobId == jobId)
            .Include(c => c.Interviews)
            .AsNoTracking()
            .ToListAsync();

        // Sort them by their computed average score in memory
        var ranked = peers
            .OrderByDescending(c => c.Interviews
                .Select(i => decimal.TryParse(i.Grade, out var g) ? (decimal?)g : null)
                .Where(g => g.HasValue)
                .Select(g => g!.Value)
                .DefaultIfEmpty(0m)
                .Average())
            .ThenBy(c => c.Id)
            .ToList();

        return ranked;
    }
}

