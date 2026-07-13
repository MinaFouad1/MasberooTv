using MassperoTVAPI.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassperoTVAPI.Infrastructure.Data.Configurations;

public class CandidateSkillConfiguration : IEntityTypeConfiguration<CandidateSkill>
{
    public void Configure(EntityTypeBuilder<CandidateSkill> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.YearsOfExperience)
               .IsRequired();

        builder.HasIndex(x => new { x.CandidateId, x.SkillId })
               .IsUnique();

        builder.HasOne(x => x.Candidate)
               .WithMany(c => c.CandidateSkills)
               .HasForeignKey(x => x.CandidateId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Skill)
               .WithMany(s => s.CandidateSkills)
               .HasForeignKey(x => x.SkillId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
