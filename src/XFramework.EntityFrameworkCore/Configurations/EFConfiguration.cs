using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using XFramework.Domain.Identity;

namespace XFramework.EntityFrameworkCore.Configurations;

public class EFConfiguration
    : IEntityTypeConfiguration
{
    public void Configure(
        EntityTypeBuilder builder)
    {
        /*for users*/
        modelBuilder.Entity<User>(builder =>
        {
            builder.ToTable("Users");

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
        });
        /** for Roles **/
        modelBuilder.Entity<Role>(builder =>
            {
                builder.ToTable("Roles");

                builder.HasKey(x => x.Id);

                builder.Property(x => x.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                builder.HasIndex(x => x.Name)
                    .IsUnique();

                builder.Property(x => x.Description)
                    .HasMaxLength(500);
            });

        /*******for Permissions*******/
        modelBuilder.Entity<Permission>(builder =>
        {
            builder.ToTable("Permissions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasIndex(x => x.Name)
                .IsUnique();

            builder.Property(x => x.DisplayName)
                .HasMaxLength(200);

            builder.Property(x => x.GroupName)
                .HasMaxLength(100);
        });
        /******** for UserRole *********/
        modelBuilder.Entity<UserRole>(builder =>
        {
            builder.ToTable("UserRoles");

            builder.HasKey(x => new
            {
                x.UserId,
                x.RoleId
            });

            builder.HasOne(x => x.User)
                .WithMany(x => x.Roles)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Role)
                .WithMany()
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        /******* for RolePermissions *********/
        modelBuilder.Entity<RolePermission>(builder =>
        {
            builder.ToTable("RolePermissions");

            builder.HasKey(x => new
            {
                x.RoleId,
                x.PermissionId
            });

            builder.HasOne(x => x.Role)
                .WithMany(x => x.Permissions)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Permission)
                .WithMany()
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        /********for UserPermissions ********/

        modelBuilder.Entity<UserPermission>(builder =>
        {
            builder.ToTable("UserPermissions");

            builder.HasKey(x => new
            {
                x.UserId,
                x.PermissionId
            });

            builder.HasOne(x => x.User)
                .WithMany(x => x.Permissions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Permission)
                .WithMany()
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
     }
}