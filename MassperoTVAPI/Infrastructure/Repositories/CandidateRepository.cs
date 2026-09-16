using MassperoTV.Infrastructure.Repositories;
using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Interfaces.Repositories;
using MassperoTVAPI.Core.Mappers;
using MassperoTVAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
        IEnumerable<UpsertCandidateSkillDto>? skills,
        IEnumerable<UpsertCandidateLanguageDto>? languages,
        IEnumerable<UpsertCandidateCertificationDto>? certifications,
        IEnumerable<UpsertCandidateEducationDto>? educations)
    {
        if (skills is not null)
        {
            _context.CandidateSkills.RemoveRange(candidate.CandidateSkills);
            candidate.CandidateSkills.Clear();

            foreach (var item in skills.Where(s => !string.IsNullOrWhiteSpace(s.Name))
                         .GroupBy(s => s.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                         .Select(g => g.First()))
            {
                var name = item.Name.Trim();
                var skill = await _context.Skills.FirstOrDefaultAsync(s => s.Name == name);
                if (skill is null)
                {
                    skill = new Skill { Name = name };
                    await _context.Skills.AddAsync(skill);
                }

                candidate.CandidateSkills.Add(new CandidateSkill
                {
                    CandidateId = candidate.Id,
                    Skill = skill,
                    YearsOfExperience = item.YearsOfExperience
                });
            }
        }

        if (languages is not null)
        {
            _context.CandidateLanguages.RemoveRange(candidate.CandidateLanguages);
            candidate.CandidateLanguages.Clear();

            foreach (var item in languages.Where(l => !string.IsNullOrWhiteSpace(l.Name))
                         .GroupBy(l => l.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                         .Select(g => g.First()))
            {
                var name = item.Name.Trim();
                var language = await _context.Languages.FirstOrDefaultAsync(l => l.Name == name);
                if (language is null)
                {
                    language = new Language { Name = name };
                    await _context.Languages.AddAsync(language);
                }

                candidate.CandidateLanguages.Add(new CandidateLanguage
                {
                    CandidateId = candidate.Id,
                    Language = language
                });
            }
        }

        if (certifications is not null)
        {
            _context.Certifications.RemoveRange(candidate.Certifications);
            candidate.Certifications.Clear();

            foreach (var item in certifications.Where(c => !string.IsNullOrWhiteSpace(c.Name)))
            {
                candidate.Certifications.Add(new Certification
                {
                    CandidateId = candidate.Id,
                    Name = item.Name.Trim(),
                    Url = string.IsNullOrWhiteSpace(item.Url) ? null : item.Url.Trim()
                });
            }
        }

        if (educations is not null)
        {
            _context.Educations.RemoveRange(candidate.Educations);
            candidate.Educations.Clear();

            foreach (var item in educations.Where(e =>
                         !string.IsNullOrWhiteSpace(e.Degree) &&
                         !string.IsNullOrWhiteSpace(e.University)))
            {
                candidate.Educations.Add(new Education
                {
                    CandidateId = candidate.Id,
                    Degree = item.Degree.Trim(),
                    University = item.University.Trim(),
                    GraduationYear = item.GraduationYear,
                    Grade = string.IsNullOrWhiteSpace(item.Grade) ? null : item.Grade.Trim()
                });
            }
        }
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

