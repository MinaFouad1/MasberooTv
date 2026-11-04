using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MassperoTVAPI.Modules.Hr.Domain.Entities.Employees;

namespace MassperoTVAPI.Infrastructure.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(x => x.EmploymentType)
               .HasMaxLength(50);

        builder.HasOne(x => x.Job)
               .WithMany()
               .HasForeignKey(x => x.JobId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);

        builder.HasOne(x => x.Team)
               .WithMany()
               .HasForeignKey(x => x.TeamId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);

        builder.HasOne(x => x.Candidate)
               .WithMany()
               .HasForeignKey(x => x.CandidateId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);

        builder.HasIndex(x => x.CandidateId)
               .IsUnique()
               .HasFilter("[CandidateId] IS NOT NULL");
    }
}
