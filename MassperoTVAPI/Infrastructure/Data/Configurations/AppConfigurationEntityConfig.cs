using MassperoTVAPI.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassperoTVAPI.Infrastructure.Data.Configurations;

public class AppConfigurationEntityConfig : IEntityTypeConfiguration<AppConfiguration>
{
    public void Configure(EntityTypeBuilder<AppConfiguration> builder)
    {
        // Maps to [dbo].[Configuration] exactly as specified
        builder.ToTable("Configuration");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
               .ValueGeneratedOnAdd(); // auto-increment

        builder.Property(c => c.Key)
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(c => c.Value)
               .IsRequired();

        // Unique index on Key — each type name must be unique
        builder.HasIndex(c => c.Key)
               .IsUnique()
               .HasDatabaseName("IX_Configuration_Key");
    }
}
