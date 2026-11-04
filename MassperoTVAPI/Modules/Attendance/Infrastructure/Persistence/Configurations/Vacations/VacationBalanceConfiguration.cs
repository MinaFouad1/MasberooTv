using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Vacations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Configurations.Vacations;

public class VacationBalanceConfiguration : IEntityTypeConfiguration<VacationBalance>
{
    public void Configure(EntityTypeBuilder<VacationBalance> builder)
    {
        builder.ToTable("VacationBalances", AttendanceSchema.Name);
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.Entitled).HasPrecision(5, 2);
        builder.Property(x => x.Used).HasPrecision(5, 2);
        builder.Property(x => x.CarriedOver).HasPrecision(5, 2);
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasOne(x => x.Employee)
               .WithMany()
               .HasForeignKey(x => x.EmployeeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.VacationType)
               .WithMany()
               .HasForeignKey(x => x.VacationTypeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.EmployeeId, x.VacationTypeId, x.Year })
               .IsUnique()
               .HasFilter("[IsDeleted] = 0");
    }
}
