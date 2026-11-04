using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MassperoTVAPI.Modules.Hr.Domain.Entities.Lookups;

namespace MassperoTVAPI.Infrastructure.Data.Configurations;

public class SecurityClearanceStatusConfiguration : IEntityTypeConfiguration<SecurityClearanceStatus>
{
    public void Configure(EntityTypeBuilder<SecurityClearanceStatus> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
               .IsRequired()
               .HasMaxLength(100);
    }
}
