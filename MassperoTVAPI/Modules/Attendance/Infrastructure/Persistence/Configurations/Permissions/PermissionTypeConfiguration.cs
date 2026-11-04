using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Configurations.Permissions;

public class PermissionTypeConfiguration : IEntityTypeConfiguration<PermissionType>
{
    public void Configure(EntityTypeBuilder<PermissionType> builder)
    {
        builder.ToTable("PermissionTypes", AttendanceSchema.Name);
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
    }
}
