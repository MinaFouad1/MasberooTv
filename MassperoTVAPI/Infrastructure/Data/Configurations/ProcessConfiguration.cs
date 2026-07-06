using MassperoTVAPI.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassperoTVAPI.Infrastructure.Data.Configurations;

public class ProcessConfiguration : IEntityTypeConfiguration<Process>
{
    public void Configure(EntityTypeBuilder<Process> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(x => x.StartDate)
               .IsRequired();

        builder.Property(x => x.EndDate)
               .IsRequired(false);

        // FK → Phase (1 Phase : M Processes)
        builder.HasOne(x => x.Phase)
               .WithMany(p => p.Processes)
               .HasForeignKey(x => x.PhaseId)
               .OnDelete(DeleteBehavior.Restrict);

        // FK → ProcessStatus
        builder.HasOne(x => x.ProcessStatus)
               .WithMany(ps => ps.Processes)
               .HasForeignKey(x => x.ProcessStatusId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
