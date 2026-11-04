using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Configurations.Permissions;

public class PermissionBalanceConfiguration : IEntityTypeConfiguration<PermissionBalance>
{
    public void Configure(EntityTypeBuilder<PermissionBalance> builder)
    {
        builder.ToTable("PermissionBalances", AttendanceSchema.Name);
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasOne(x => x.Employee)
               .WithMany()
               .HasForeignKey(x => x.EmployeeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PermissionType)
               .WithMany()
               .HasForeignKey(x => x.PermissionTypeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.EmployeeId, x.PermissionTypeId, x.Year, x.Month })
               .IsUnique()
               .HasFilter("[IsDeleted] = 0");
    }
}
