using MassperoTVAPI.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassperoTVAPI.Infrastructure.Data.Configurations;

public class InterviewConfiguration : IEntityTypeConfiguration<Interview>
{
    public void Configure(EntityTypeBuilder<Interview> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Grade)
               .HasMaxLength(50);

        builder.Property(x => x.Comments)
               .HasMaxLength(2000);

        // FK → Candidate
        builder.HasOne(x => x.Candidate)
               .WithMany(c => c.Interviews)
               .HasForeignKey(x => x.CandidateId)
               .OnDelete(DeleteBehavior.Cascade);

        // FK → InterviewType
        builder.HasOne(x => x.Type)
               .WithMany(t => t.Interviews)
               .HasForeignKey(x => x.TypeId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
