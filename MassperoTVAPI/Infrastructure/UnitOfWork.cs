using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Interfaces;
using MassperoTVAPI.Core.Interfaces.Repositories;
using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Infrastructure.Repositories;

namespace MassperoTVAPI.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public ICandidateRepository              Candidates         { get; }
    public IInterviewRepository              Interviews         { get; }
    public IJobRepository                    Jobs               { get; }
    public ICategoryRepository               Categories         { get; }
    public IIssueRepository                  Issues             { get; }
    public IProcessRepository                Processes          { get; }
    public ILookupRepository<Status>                 Statuses           { get; }
    public ILookupRepository<SecurityClearanceStatus> SecurityClearances { get; }
    public ILookupRepository<InterviewType>          InterviewTypes     { get; }
    public ILookupRepository<Phase>                  Phases             { get; }
    public ILookupRepository<ProcessStatus>          ProcessStatuses    { get; }
    public ILookupRepository<Location>               Locations          { get; }
    public IConfigurationRepository          Configurations     { get; }

    public UnitOfWork(
        ApplicationDbContext               context,
        ICandidateRepository               candidates,
        IInterviewRepository               interviews,
        IJobRepository                     jobs,
        ICategoryRepository                categories,
        IIssueRepository                   issues,
        IProcessRepository                 processes,
        ILookupRepository<Status>                  statuses,
        ILookupRepository<SecurityClearanceStatus>  securityClearances,
        ILookupRepository<InterviewType>           interviewTypes,
        ILookupRepository<Phase>                   phases,
        ILookupRepository<ProcessStatus>           processStatuses,
        ILookupRepository<Location>                locations,
        IConfigurationRepository           configurations)
    {
        _context           = context;
        Candidates         = candidates;
        Interviews         = interviews;
        Jobs               = jobs;
        Categories         = categories;
        Issues             = issues;
        Processes          = processes;
        Statuses           = statuses;
        SecurityClearances = securityClearances;
        InterviewTypes     = interviewTypes;
        Phases             = phases;
        ProcessStatuses    = processStatuses;
        Locations          = locations;
        Configurations     = configurations;
    }

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public void Dispose()
        => _context.Dispose();
}
