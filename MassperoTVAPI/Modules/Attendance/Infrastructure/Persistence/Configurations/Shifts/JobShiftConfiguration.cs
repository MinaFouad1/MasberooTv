using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Shifts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Configurations.Shifts;

public class JobShiftConfiguration : IEntityTypeConfiguration<JobShift>
{
    public void Configure(EntityTypeBuilder<JobShift> builder)
    {
        builder.ToTable("JobShifts", AttendanceSchema.Name);
        builder.HasKey(x => new { x.JobId, x.ShiftId });
        builder.ConfigureAuditable();

        builder.HasOne(x => x.Job)
               .WithMany()
               .HasForeignKey(x => x.JobId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Shift)
               .WithMany()
               .HasForeignKey(x => x.ShiftId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
