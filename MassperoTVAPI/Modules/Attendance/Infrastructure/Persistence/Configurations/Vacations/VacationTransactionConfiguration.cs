using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Vacations;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Configurations.Vacations;

public class VacationTransactionConfiguration : IEntityTypeConfiguration<VacationTransaction>
{
    public void Configure(EntityTypeBuilder<VacationTransaction> builder)
    {
        builder.ToTable("VacationTransactions", AttendanceSchema.Name, t =>
            t.HasCheckConstraint(
                "CK_VacationTransactions_Request_Or_Attendance",
                "NOT ([VacationRequestId] IS NOT NULL AND [AttendanceId] IS NOT NULL)"));
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.Days).HasPrecision(5, 2);
        builder.Property(x => x.Reason)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.HasOne(x => x.VacationBalance)
               .WithMany()
               .HasForeignKey(x => x.VacationBalanceId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.VacationRequest)
               .WithMany()
               .HasForeignKey(x => x.VacationRequestId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);

        builder.HasOne(x => x.Attendance)
               .WithMany()
               .HasForeignKey(x => x.AttendanceId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);
    }
}
