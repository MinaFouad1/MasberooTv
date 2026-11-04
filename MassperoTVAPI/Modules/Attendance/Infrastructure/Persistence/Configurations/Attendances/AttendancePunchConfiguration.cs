using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Attendances;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Configurations.Attendances;

public class AttendancePunchConfiguration : IEntityTypeConfiguration<AttendancePunch>
{
    public void Configure(EntityTypeBuilder<AttendancePunch> builder)
    {
        builder.ToTable("AttendancePunches", AttendanceSchema.Name);
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.DeviceId).HasMaxLength(100);
        builder.Property(x => x.Source)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.HasOne(x => x.Employee)
               .WithMany()
               .HasForeignKey(x => x.EmployeeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.EmployeeId, x.PunchTime });
        builder.HasIndex(x => x.IsProcessed);
    }
}
