using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Vacations;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Configurations.Vacations;

public class VacationRequestConfiguration : IEntityTypeConfiguration<VacationRequest>
{
    public void Configure(EntityTypeBuilder<VacationRequest> builder)
    {
        builder.ToTable("VacationRequests", AttendanceSchema.Name);
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.FromDate).HasColumnType("date");
        builder.Property(x => x.ToDate).HasColumnType("date");
        builder.Property(x => x.Days).HasPrecision(5, 2);
        builder.Property(x => x.Status)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();
        builder.Property(x => x.Reason).IsRequired().HasMaxLength(500);
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasOne(x => x.Employee)
               .WithMany()
               .HasForeignKey(x => x.EmployeeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.VacationType)
               .WithMany()
               .HasForeignKey(x => x.VacationTypeId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
