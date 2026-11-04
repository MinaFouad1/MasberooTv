using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MassperoTVAPI.Modules.Hr.Domain.Entities.Identity;

namespace MassperoTVAPI.Modules.Hr.Infrastructure.Persistence.Configurations.Identity;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.IsVerified).HasDefaultValue(false);
    }
}
