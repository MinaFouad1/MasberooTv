using MassperoTVAPI.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassperoTVAPI.Infrastructure.Data.Configurations;

public class ProcessStatusConfiguration : IEntityTypeConfiguration<ProcessStatus>
{
    public void Configure(EntityTypeBuilder<ProcessStatus> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(x => x.Percentage)
               .HasPrecision(5, 2);

        // Seed the 4 required statuses
        builder.HasData(
            new ProcessStatus { Id = 1, Name = "Pending",    Percentage = 0m   },
            new ProcessStatus { Id = 2, Name = "InProgress", Percentage = 50m  },
            new ProcessStatus { Id = 3, Name = "Completed",  Percentage = 100m },
            new ProcessStatus { Id = 4, Name = "Holded",     Percentage = 0m   }
        );
    }
}
