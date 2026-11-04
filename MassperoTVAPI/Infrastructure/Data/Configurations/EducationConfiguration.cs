using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MassperoTVAPI.Modules.Hr.Domain.Entities.Candidates;

namespace MassperoTVAPI.Infrastructure.Data.Configurations;

public class EducationConfiguration : IEntityTypeConfiguration<Education>
{
    public void Configure(EntityTypeBuilder<Education> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Degree)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(x => x.University)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(x => x.GraduationYear)
               .IsRequired();

        builder.Property(x => x.Grade)
               .HasMaxLength(100);

        builder.HasOne(x => x.Candidate)
               .WithMany(c => c.Educations)
               .HasForeignKey(x => x.CandidateId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
