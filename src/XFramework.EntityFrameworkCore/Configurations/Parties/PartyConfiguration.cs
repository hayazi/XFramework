using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using XFramework.Domain.Parties;
using XFramework.Domain.SharedKernel;
using XFramework.EntityFrameworkCore.ValueConverters;
using System.Text.Json;
using System.Collections.Generic;

namespace XFramework.EntityFrameworkCore.Configurations.Parties;

public class PartyConfiguration : IEntityTypeConfiguration<Party>
{
    public void Configure(EntityTypeBuilder<Party> builder)
    {
        builder.ToTable("Parties");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.TaxId)
            .HasMaxLength(50);

        builder.Property(x => x.NationalId)
            .HasMaxLength(50);

        builder.Property(x => x.IsActive)
            .IsRequired();

        var contactConverter = new ValueConverter<ContactInfo, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<ContactInfo>(v, (JsonSerializerOptions?)null)!);

        var contactComparer = new ValueComparer<ContactInfo>(
            (c1, c2) => c1 != null && c2 != null &&
                c1.Name == c2.Name &&
                c1.Phone == c2.Phone &&
                c1.Email == c2.Email &&
                Equals(c1.Address, c2.Address),
            c => HashCode.Combine(c.Name, c.Phone, c.Email, c.Address),
            c => new ContactInfo(c.Name, c.Phone, c.Email, c.Address));

        builder.Property(x => x.Contact)
            .HasConversion(contactConverter)
            .HasColumnType("nvarchar(max)")
            .IsRequired()
            .Metadata.SetValueComparer(contactComparer);

        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.TaxId);
        builder.HasIndex(x => x.NationalId);
    }
}

public class PartyRoleAssignmentConfiguration : IEntityTypeConfiguration<PartyRoleAssignment>
{
    public void Configure(EntityTypeBuilder<PartyRoleAssignment> builder)
    {
        builder.ToTable("PartyRoleAssignments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PartyId)
            .IsRequired();

        builder.Property(x => x.Role)
            .IsRequired();

        builder.Property(x => x.ValidFrom)
            .IsRequired();

        builder.Property(x => x.ValidTo);

        // PartyRoleAssignment doesn't have a Party navigation property in the domain
        // The relationship is maintained via PartyId foreign key only

        builder.HasIndex(x => x.PartyId);
        builder.HasIndex(x => x.Role);
        builder.HasIndex(x => new { x.ValidFrom, x.ValidTo });
    }
}