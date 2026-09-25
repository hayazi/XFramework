using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using XFramework.Domain.Dimensions;
using XFramework.Domain.SharedKernel;

namespace XFramework.EntityFrameworkCore.Configurations.Dimensions;

public class CostCenterConfiguration : IEntityTypeConfiguration<CostCenter>
{
    public void Configure(EntityTypeBuilder<CostCenter> builder)
    {
        builder.ToTable("CostCenters");

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

        builder.Property(x => x.ParentCostCenterId);

        builder.Property(x => x.ManagerId)
            .HasMaxLength(100);

        builder.Property(x => x.BudgetAmount)
            .HasPrecision(18, 4);

        builder.Property(x => x.BudgetCurrency);

        builder.HasOne(x => x.ParentCostCenter)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentCostCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ParentCostCenterId);
        builder.HasIndex(x => x.ManagerId);
    }
}