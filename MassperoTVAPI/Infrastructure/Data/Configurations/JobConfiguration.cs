using MassperoTVAPI.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassperoTVAPI.Infrastructure.Data.Configurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(x => x.Desc)
               .HasMaxLength(1000);

        builder.Property(x => x.EmploymentType)
               .HasMaxLength(100);

        builder.HasOne(x => x.Category)
               .WithMany(c => c.Jobs)
               .HasForeignKey(x => x.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Location)
               .WithMany(l => l.Jobs)
               .HasForeignKey(x => x.LocationId)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);
    }
}

