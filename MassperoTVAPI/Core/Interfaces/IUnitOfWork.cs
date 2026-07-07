using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Interfaces.Repositories;

namespace MassperoTVAPI.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Domain repositories
    ICandidateRepository  Candidates  { get; }
    IInterviewRepository  Interviews  { get; }
    IJobRepository        Jobs        { get; }
    ICategoryRepository   Categories  { get; }
    IIssueRepository      Issues      { get; }
    IProcessRepository    Processes   { get; }
    IOfferRepository      Offers      { get; }

    // Lookup repositories
    ILookupRepository<Status>                 Statuses          { get; }
    ILookupRepository<SecurityClearanceStatus> SecurityClearances { get; }
    ILookupRepository<InterviewType>          InterviewTypes    { get; }
    ILookupRepository<Phase>                  Phases            { get; }
    ILookupRepository<ProcessStatus>          ProcessStatuses   { get; }
    ILookupRepository<Location>               Locations         { get; }

    // Config
    IConfigurationRepository Configurations { get; }

    Task<int> SaveChangesAsync();
}
