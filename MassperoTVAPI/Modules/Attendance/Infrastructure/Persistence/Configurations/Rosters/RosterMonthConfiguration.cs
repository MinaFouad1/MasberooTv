using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Rosters;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Configurations.Rosters;

public class RosterMonthConfiguration : IEntityTypeConfiguration<RosterMonth>
{
    public void Configure(EntityTypeBuilder<RosterMonth> builder)
    {
        builder.ToTable("RosterMonths", AttendanceSchema.Name);
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.Status)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.HasOne(x => x.Team)
               .WithMany()
               .HasForeignKey(x => x.TeamId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);

        builder.HasIndex(x => new { x.TeamId, x.Year, x.Month })
               .IsUnique()
               .HasFilter("[IsDeleted] = 0");
    }
}
