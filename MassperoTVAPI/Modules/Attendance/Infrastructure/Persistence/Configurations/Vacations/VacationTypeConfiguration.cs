using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Vacations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Configurations.Vacations;

public class VacationTypeConfiguration : IEntityTypeConfiguration<VacationType>
{
    public void Configure(EntityTypeBuilder<VacationType> builder)
    {
        builder.ToTable("VacationTypes", AttendanceSchema.Name);
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
    }
}
