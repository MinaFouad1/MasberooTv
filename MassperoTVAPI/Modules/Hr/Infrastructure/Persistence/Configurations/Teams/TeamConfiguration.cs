using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MassperoTVAPI.Modules.Hr.Domain.Entities.Teams;

namespace MassperoTVAPI.Modules.Hr.Infrastructure.Persistence.Configurations.Teams;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TeamName)
               .IsRequired()
               .HasMaxLength(100);
    }
}
