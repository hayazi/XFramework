using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using XFramework.Domain.Dimensions;
using XFramework.EntityFrameworkCore.ValueConverters;

namespace XFramework.EntityFrameworkCore.Configurations.Dimensions;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.Property(x => x.EndDate);

        builder.Property(x => x.ActualEndDate);

        builder.Property(x => x.Budget)
            .HasConversion(new MoneyNullableConverter())
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.ManagerId);

        builder.Property(x => x.CustomerId);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.StartDate);
        builder.HasIndex(x => x.ManagerId);
        builder.HasIndex(x => x.CustomerId);
    }
}