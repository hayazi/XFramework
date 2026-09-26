using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using XFramework.Domain.Identity;

namespace XFramework.EntityFrameworkCore.Configurations.Identity;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("XfUsers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserName)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.UserName)
            .IsUnique();

        builder.Property(x => x.DisplayName)
            .HasMaxLength(200);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("XfRoles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Property(x => x.DisplayName)
            .HasMaxLength(200);
    }
}

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("XfPermissions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Property(x => x.DisplayName)
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(500);
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("XfUserRoles");

        builder.HasKey(x => new { x.UserId, x.RoleId });

        builder.HasOne(x => x.User)
            .WithMany(x => x.Roles)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("XfRolePermissions");

        builder.HasKey(x => new { x.RoleId, x.PermissionId });

        builder.HasOne(x => x.Role)
            .WithMany(x => x.Permissions)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Permission)
            .WithMany(x => x.Roles)
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.ToTable("XfUserPermissions");

        builder.HasKey(x => new { x.UserId, x.PermissionId });

        builder.HasOne(x => x.User)
            .WithMany(x => x.Permissions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Permission)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}