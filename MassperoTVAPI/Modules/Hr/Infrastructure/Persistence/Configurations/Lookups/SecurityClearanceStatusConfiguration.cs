using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MassperoTVAPI.Modules.Hr.Domain.Entities.Lookups;

namespace MassperoTVAPI.Modules.Hr.Infrastructure.Persistence.Configurations.Lookups;

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
