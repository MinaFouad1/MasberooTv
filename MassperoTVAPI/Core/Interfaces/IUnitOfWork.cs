using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Interfaces.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;

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
    ILookupRepository<Language>               Languages         { get; }
    ILookupRepository<Skill>                  Skills            { get; }

    // Config
    IConfigurationRepository Configurations { get; }

    // Attendance repositories
    IShiftRepository Shifts { get; }
    IRosterMonthRepository RosterMonths { get; }
    IShiftAssignmentRepository ShiftAssignments { get; }
    IAttendancePunchRepository AttendancePunches { get; }
    IAttendanceRepository Attendances { get; }
    IVacationTypeRepository VacationTypes { get; }
    IVacationRequestRepository VacationRequests { get; }
    IVacationBalanceRepository VacationBalances { get; }
    IHolidayRepository Holidays { get; }
    IPermissionTypeRepository PermissionTypes { get; }
    IPermissionBalanceRepository PermissionBalances { get; }
    IPermissionRequestRepository PermissionRequests { get; }

    Task<int> SaveChangesAsync();
}
