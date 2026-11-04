using MassperoTVAPI.Modules.AttendanceModule.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Configurations;

public static class AuditableConfigurationExtensions
{
    public static EntityTypeBuilder<T> ConfigureAuditable<T>(this EntityTypeBuilder<T> builder)
        where T : AuditableBase
    {
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
        return builder;
    }

    public static EntityTypeBuilder<T> ConfigureAuditableEntity<T>(this EntityTypeBuilder<T> builder)
        where T : AuditableEntity
    {
        builder.HasKey(x => x.Id);
        return builder.ConfigureAuditable();
    }
}
