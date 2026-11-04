using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Shifts;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Rosters;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Attendances;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Vacations;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Permissions;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Holidays;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MassperoTVAPI.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // ── Lookup tables ─────────────────────────────────────────────────────────
    public DbSet<AppConfiguration>       Configurations            => Set<AppConfiguration>();
    public DbSet<SecurityClearanceStatus> SecurityClearanceStatuses => Set<SecurityClearanceStatus>();
    public DbSet<Status>                 Statuses                  => Set<Status>();
    public DbSet<Category>               Categories                => Set<Category>();
    public DbSet<InterviewType>          InterviewTypes            => Set<InterviewType>();
    public DbSet<Phase>                  Phases                    => Set<Phase>();
    public DbSet<ProcessStatus>          ProcessStatuses           => Set<ProcessStatus>();
    public DbSet<Location>               Locations                 => Set<Location>();
    public DbSet<Skill>                  Skills                    => Set<Skill>();
    public DbSet<Language>               Languages                 => Set<Language>();

    // ── Main entities ─────────────────────────────────────────────────────────
    public DbSet<Job>                    Jobs                    => Set<Job>();
    public DbSet<Candidate>              Candidates              => Set<Candidate>();
    public DbSet<Interview>              Interviews              => Set<Interview>();
    public DbSet<CandidateSkill>          CandidateSkills          => Set<CandidateSkill>();
    public DbSet<CandidateLanguage>       CandidateLanguages       => Set<CandidateLanguage>();
    public DbSet<Certification>           Certifications           => Set<Certification>();
    public DbSet<Education>               Educations               => Set<Education>();
    public DbSet<Process>                Processes               => Set<Process>();
    public DbSet<ProcessDependency>      ProcessDependencies     => Set<ProcessDependency>();
    public DbSet<Issue>                  Issues                  => Set<Issue>();
    public DbSet<Offer>                  Offers                  => Set<Offer>();

    // HR: Employees / Teams
    public DbSet<Employee>               Employees               => Set<Employee>();
    public DbSet<Team>                   Teams                   => Set<Team>();

    // Attendance module
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<JobShift> JobShifts => Set<JobShift>();
    public DbSet<RosterMonth> RosterMonths => Set<RosterMonth>();
    public DbSet<ShiftAssignment> ShiftAssignments => Set<ShiftAssignment>();
    public DbSet<AttendancePunch> AttendancePunches => Set<AttendancePunch>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<VacationType> VacationTypes => Set<VacationType>();
    public DbSet<VacationRequest> VacationRequests => Set<VacationRequest>();
    public DbSet<VacationBalance> VacationBalances => Set<VacationBalance>();
    public DbSet<VacationTransaction> VacationTransactions => Set<VacationTransaction>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<PermissionType> PermissionTypes => Set<PermissionType>();
    public DbSet<PermissionBalance> PermissionBalances => Set<PermissionBalance>();
    public DbSet<PermissionRequest> PermissionRequests => Set<PermissionRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration classes in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
