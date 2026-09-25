using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using XFramework.Domain.Inventory;
using XFramework.EntityFrameworkCore.ValueConverters;

namespace XFramework.EntityFrameworkCore.Configurations.Inventory;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");

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

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.Address)
            .HasConversion(new AddressConverter())
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.AllowNegativeStock)
            .IsRequired();

        builder.Property(x => x.ManagerId);

        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.ManagerId);
    }
}