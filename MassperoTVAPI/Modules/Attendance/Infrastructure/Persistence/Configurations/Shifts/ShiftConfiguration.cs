using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Shifts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Configurations.Shifts;

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.ToTable("Shifts", AttendanceSchema.Name);
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.StartTime).HasColumnType("time");
        builder.Property(x => x.TimeZoneId)
               .IsRequired()
               .HasMaxLength(100)
               .HasDefaultValue("Egypt Standard Time");
    }
}
