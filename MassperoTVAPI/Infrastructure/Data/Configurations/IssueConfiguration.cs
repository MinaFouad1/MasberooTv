using MassperoTVAPI.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassperoTVAPI.Infrastructure.Data.Configurations;

public class IssueConfiguration : IEntityTypeConfiguration<Issue>
{
    public void Configure(EntityTypeBuilder<Issue> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
               .IsRequired()
               .HasMaxLength(300);

        // FK → Process (M Issues : 1 Process)
        builder.HasOne(x => x.Process)
               .WithMany(p => p.Issues)
               .HasForeignKey(x => x.ProcessId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
