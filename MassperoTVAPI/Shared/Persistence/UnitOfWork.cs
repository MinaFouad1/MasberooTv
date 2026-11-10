using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Modules.Hr.Domain.Entities.Lookups;
using MassperoTVAPI.Modules.Hr.Application.Abstractions.Repositories;

namespace MassperoTVAPI.Shared.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public ICandidateRepository              Candidates         { get; }
    public IInterviewRepository              Interviews         { get; }
    public IJobRepository                    Jobs               { get; }
    public ICategoryRepository               Categories         { get; }
    public IIssueRepository                  Issues             { get; }
    public IProcessRepository                Processes          { get; }
    public IOfferRepository                  Offers             { get; }
    public IEmployeeRepository                  Employee             { get; }
    public ILookupRepository<Status>                 Statuses           { get; }
    public ILookupRepository<SecurityClearanceStatus> SecurityClearances { get; }
    public ILookupRepository<InterviewType>          InterviewTypes     { get; }
    public ILookupRepository<Phase>                  Phases             { get; }
    public ILookupRepository<ProcessStatus>          ProcessStatuses    { get; }
    public ILookupRepository<Location>               Locations          { get; }
    public ILookupRepository<Language>               Languages          { get; }
    public ILookupRepository<Skill>                  Skills             { get; }
    public IConfigurationRepository          Configurations     { get; }

    // Attendance repositories
    public IShiftRepository Shifts { get; }
    public IRosterMonthRepository RosterMonths { get; }
    public IShiftAssignmentRepository ShiftAssignments { get; }
    public IAttendancePunchRepository AttendancePunches { get; }
    public IAttendanceRepository Attendances { get; }
    public IVacationTypeRepository VacationTypes { get; }
    public IVacationRequestRepository VacationRequests { get; }
    public IVacationBalanceRepository VacationBalances { get; }
    public IHolidayRepository Holidays { get; }
    public IPermissionTypeRepository PermissionTypes { get; }
    public IPermissionBalanceRepository PermissionBalances { get; }
    public IPermissionRequestRepository PermissionRequests { get; }

    public UnitOfWork(
        ApplicationDbContext               context,
        ICandidateRepository               candidates,
        IInterviewRepository               interviews,
        IJobRepository                     jobs,
        ICategoryRepository                categories,
        IIssueRepository                   issues,
        IProcessRepository                 processes,
        IOfferRepository                   offers,
        IEmployeeRepository                employee,
        ILookupRepository<Status>                  statuses,
        ILookupRepository<SecurityClearanceStatus>  securityClearances,
        ILookupRepository<InterviewType>           interviewTypes,
        ILookupRepository<Phase>                   phases,
        ILookupRepository<ProcessStatus>           processStatuses,
        ILookupRepository<Location>                locations,
        ILookupRepository<Language>                languages,
        ILookupRepository<Skill>                   skills,
        IShiftRepository shifts,
        IRosterMonthRepository rosterMonths,
        IShiftAssignmentRepository shiftAssignments,
        IAttendancePunchRepository attendancePunches,
        IAttendanceRepository attendances,
        IVacationTypeRepository vacationTypes,
        IVacationRequestRepository vacationRequests,
        IVacationBalanceRepository vacationBalances,
        IHolidayRepository holidays,
        IPermissionTypeRepository permissionTypes,
        IPermissionBalanceRepository permissionBalances,
        IPermissionRequestRepository permissionRequests,
        IConfigurationRepository           configurations)
    {
        _context           = context;
        Candidates         = candidates;
        Interviews         = interviews;
        Jobs               = jobs;
        Categories         = categories;
        Issues             = issues;
        Processes          = processes;
        Offers             = offers;
        Employee           =employee;
        Statuses           = statuses;
        SecurityClearances = securityClearances;
        InterviewTypes     = interviewTypes;
        Phases             = phases;
        ProcessStatuses    = processStatuses;
        Locations          = locations;
        Languages          = languages;
        Skills             = skills;
        Configurations     = configurations;
        Shifts = shifts;
        RosterMonths = rosterMonths;
        ShiftAssignments = shiftAssignments;
        AttendancePunches = attendancePunches;
        Attendances = attendances;
        VacationTypes = vacationTypes;
        VacationRequests = vacationRequests;
        VacationBalances = vacationBalances;
        Holidays = holidays;
        PermissionTypes = permissionTypes;
        PermissionBalances = permissionBalances;
        PermissionRequests = permissionRequests;
    }

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public void Dispose()
        => _context.Dispose();
}
