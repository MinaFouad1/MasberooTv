using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassperoTVAPI.Infrastructure.Data.Configurations;

public class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        builder.ToTable("Offers");

        builder.HasKey(x => x.Id);

        // Store enum as int column named OfferStatusId
        builder.Property(x => x.OfferStatusId)
               .HasColumnName("OfferStatusId")
               .HasConversion<int>()
               .IsRequired();

        builder.Property(x => x.ProposedSalary)
               .HasColumnType("decimal(12,2)")
               .IsRequired();

        builder.Property(x => x.OfferDate)
               .HasColumnType("datetime2(7)")
               .HasDefaultValueSql("SYSDATETIME()")
               .IsRequired();

        builder.Property(x => x.ExpiryDate)
               .HasColumnType("datetime2(7)")
               .IsRequired();

        builder.Property(x => x.StartDate)
               .HasColumnType("datetime2(7)")
               .IsRequired(false);

        builder.Property(x => x.Benefits)
               .HasMaxLength(2000)
               .IsRequired(false);

        builder.Property(x => x.CreatedAt)
               .HasColumnType("datetime2(7)")
               .HasDefaultValueSql("SYSDATETIME()")
               .IsRequired();

        builder.Property(x => x.CreatedBy)
               .HasMaxLength(450)
               .IsRequired(false);

        // FK → Candidates (Restrict delete so we don't lose offer history)
        builder.HasOne(x => x.Candidate)
               .WithMany()
               .HasForeignKey(x => x.CandidateId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
