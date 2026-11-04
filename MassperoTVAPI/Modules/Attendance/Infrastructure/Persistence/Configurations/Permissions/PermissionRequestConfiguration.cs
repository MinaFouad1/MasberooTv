using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Permissions;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Configurations.Permissions;

public class PermissionRequestConfiguration : IEntityTypeConfiguration<PermissionRequest>
{
    public void Configure(EntityTypeBuilder<PermissionRequest> builder)
    {
        builder.ToTable("PermissionRequests", AttendanceSchema.Name);
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.PermissionDate).HasColumnType("date");
        builder.Property(x => x.Reason).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Status)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.HasOne(x => x.Employee)
               .WithMany()
               .HasForeignKey(x => x.EmployeeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PermissionType)
               .WithMany()
               .HasForeignKey(x => x.PermissionTypeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Attendance)
               .WithMany()
               .HasForeignKey(x => x.AttendanceId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);
    }
}
