using MassperoTVAPI.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassperoTVAPI.Infrastructure.Data.Configurations;

public class ProcessDependencyConfiguration : IEntityTypeConfiguration<ProcessDependency>
{
    public void Configure(EntityTypeBuilder<ProcessDependency> builder)
    {
        // Composite primary key
        builder.HasKey(x => new { x.ProcessId, x.DependsOnProcessId });

        // FK: the process that HAS the dependency
        builder.HasOne(x => x.Process)
               .WithMany(p => p.Dependencies)
               .HasForeignKey(x => x.ProcessId)
               .OnDelete(DeleteBehavior.Restrict);

        // FK: the process that must be completed first
        builder.HasOne(x => x.DependsOn)
               .WithMany(p => p.DependentProcesses)
               .HasForeignKey(x => x.DependsOnProcessId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
