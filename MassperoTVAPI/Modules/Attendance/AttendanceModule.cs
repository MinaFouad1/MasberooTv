using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Shifts;
using MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Rosters;
using MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Attendances;
using MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Vacations;
using MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Holidays;
using MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Permissions;
using Microsoft.Extensions.DependencyInjection;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Shifts;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Holidays;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Vacations;
using MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Employees;
using MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Permissions;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Rosters;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Teams;
using MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Teams;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Attendances;

namespace MassperoTVAPI.Modules.AttendanceModule;

public static class AttendanceModuleExtensions
{
    public static IServiceCollection AddAttendanceModule(this IServiceCollection services)
    {
        services.AddScoped<IShiftRepository, ShiftRepository>();
        services.AddScoped<IRosterMonthRepository, RosterMonthRepository>();
        services.AddScoped<IShiftAssignmentRepository, ShiftAssignmentRepository>();
        services.AddScoped<IAttendancePunchRepository, AttendancePunchRepository>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<IVacationTypeRepository, VacationTypeRepository>();
        services.AddScoped<IVacationRequestRepository, VacationRequestRepository>();
        services.AddScoped<IVacationBalanceRepository, VacationBalanceRepository>();
        services.AddScoped<IHolidayRepository, HolidayRepository>();
        services.AddScoped<IPermissionTypeRepository, PermissionTypeRepository>();
        services.AddScoped<IPermissionBalanceRepository, PermissionBalanceRepository>();
        services.AddScoped<IPermissionRequestRepository, PermissionRequestRepository>();

        // Lookups and replaceable business rules
        services.AddScoped<IEmployeeLookup, EmployeeLookup>();
        services.AddScoped<IVacationDaysCalculator, DefaultVacationDaysCalculator>();
        services.AddScoped<IAttendanceSaveChanges, AttendanceSaveChanges>();

        // Application services
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<IHolidayService, HolidayService>();
        services.AddScoped<IVacationTypeService, VacationTypeService>();
        services.AddScoped<IVacationBalanceService, VacationBalanceService>();
        services.AddScoped<IVacationRequestService, VacationRequestService>();

        // Permissions, Rosters, Teams
        services.AddScoped<ITeamMembershipRepository, TeamMembershipRepository>();
        services.AddScoped<IConflictAwareSaveChanges, ConflictAwareSaveChanges>();
        services.AddScoped<IPermissionUsageCalculator, DefaultPermissionUsageCalculator>();
        services.AddScoped<IShiftOverlapChecker, DefaultShiftOverlapChecker>();
        services.AddScoped<IPermissionTypeService, PermissionTypeService>();
        services.AddScoped<IPermissionBalanceService, PermissionBalanceService>();
        services.AddScoped<IPermissionRequestService, PermissionRequestService>();
        services.AddScoped<IRosterService, RosterService>();
        services.AddScoped<ITeamService, TeamService>();

        // Attendance (manual creation + search)
        services.AddScoped<IShiftTimeConverter, DefaultShiftTimeConverter>();
        services.AddOptions<AttendanceStatusOptions>();
        services.AddScoped<IAttendanceCalculator, DefaultAttendanceCalculator>();
        services.AddScoped<IAttendanceService, AttendanceService>();

        return services;
    }
}
