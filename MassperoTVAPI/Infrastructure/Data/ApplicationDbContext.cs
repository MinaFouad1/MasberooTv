using MassperoTVAPI.Core.Entities;
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

    // ── Main entities ─────────────────────────────────────────────────────────
    public DbSet<Job>                    Jobs                    => Set<Job>();
    public DbSet<Candidate>              Candidates              => Set<Candidate>();
    public DbSet<Interview>              Interviews              => Set<Interview>();
    public DbSet<Process>                Processes               => Set<Process>();
    public DbSet<ProcessDependency>      ProcessDependencies     => Set<ProcessDependency>();
    public DbSet<Issue>                  Issues                  => Set<Issue>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration classes in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
