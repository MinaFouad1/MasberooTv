using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Attendances;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Shifts;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Configurations.Attendances;

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.ToTable("Attendances", AttendanceSchema.Name);
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.WorkDate).HasColumnType("date");
        builder.Property(x => x.Status)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();
        builder.Property(x => x.OverrideReason).HasMaxLength(500);
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasOne(x => x.Employee)
               .WithMany()
               .HasForeignKey(x => x.EmployeeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Shift)
               .WithMany()
               .HasForeignKey(x => x.ShiftId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);

        builder.HasIndex(x => new { x.EmployeeId, x.WorkDate, x.ShiftId })
               .IsUnique()
               .HasFilter("[IsDeleted] = 0");
    }
}
