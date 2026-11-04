using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Holidays;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Configurations.Holidays;

public class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.ToTable("Holidays", AttendanceSchema.Name);
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
        builder.Property(x => x.HolidayDate).HasColumnType("date");

        builder.HasIndex(x => x.HolidayDate)
               .IsUnique()
               .HasFilter("[IsDeleted] = 0");
    }
}
