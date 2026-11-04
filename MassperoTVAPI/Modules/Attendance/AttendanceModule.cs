using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Shifts;
using MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Rosters;
using MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Attendances;
using MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Vacations;
using MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Holidays;
using MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Permissions;
using Microsoft.Extensions.DependencyInjection;

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

        return services;
    }
}
