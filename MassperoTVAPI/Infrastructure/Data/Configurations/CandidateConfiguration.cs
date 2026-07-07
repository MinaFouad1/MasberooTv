using MassperoTVAPI.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassperoTVAPI.Infrastructure.Data.Configurations;

public class CandidateConfiguration : IEntityTypeConfiguration<Candidate>
{
    public void Configure(EntityTypeBuilder<Candidate> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(x => x.CvFile)
               .HasMaxLength(500);

        builder.Property(x => x.Profile)
               .HasMaxLength(500);

        builder.Property(x => x.ReasonOfAccept)
               .HasMaxLength(1000);

        builder.Property(x => x.ReasonOfReject)
               .HasMaxLength(1000);

        // FK → Job (1 Job : M Candidates)
        builder.HasOne(x => x.Job)
               .WithMany(j => j.Candidates)
               .HasForeignKey(x => x.JobId)
               .OnDelete(DeleteBehavior.Restrict);

        // FK → Status
        builder.HasOne(x => x.Status)
               .WithMany(s => s.Candidates)
               .HasForeignKey(x => x.StatusId)
               .OnDelete(DeleteBehavior.Restrict);

        // FK → SecurityClearanceStatus
        builder.HasOne(x => x.SecurityClearance)
               .WithMany(sc => sc.Candidates)
               .HasForeignKey(x => x.SecurityClearanceId)
               .OnDelete(DeleteBehavior.Restrict);

        // FK → ApplicationUser (nullable — the HR user assigned to this candidate)
        // Use HasForeignKey with the string overload to explicitly bind to our declared property
        // and avoid EF generating a shadow-state 'ApplicationUserId1' column.
        builder.HasOne(x => x.User)
               .WithMany(u => u.Candidates)
               .HasForeignKey(x => x.ApplicationUserId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
