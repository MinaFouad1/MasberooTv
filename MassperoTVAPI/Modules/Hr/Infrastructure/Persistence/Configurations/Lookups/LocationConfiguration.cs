using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MassperoTVAPI.Modules.Hr.Domain.Entities.Lookups;

namespace MassperoTVAPI.Modules.Hr.Infrastructure.Persistence.Configurations.Lookups;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
               .IsRequired()
               .HasMaxLength(200);
    }
}
