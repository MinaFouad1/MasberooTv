using MassperoTVAPI.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassperoTVAPI.Infrastructure.Data.Configurations;

public class CandidateLanguageConfiguration : IEntityTypeConfiguration<CandidateLanguage>
{
    public void Configure(EntityTypeBuilder<CandidateLanguage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.CandidateId, x.LanguageId })
               .IsUnique();

        builder.HasOne(x => x.Candidate)
               .WithMany(c => c.CandidateLanguages)
               .HasForeignKey(x => x.CandidateId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Language)
               .WithMany(l => l.CandidateLanguages)
               .HasForeignKey(x => x.LanguageId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
