using Microsoft.EntityFrameworkCore;

namespace XFramework.EntityFrameworkCore.Persistence;

public class XFrameworkDbContext : DbContext
{
    public XFrameworkDbContext(
        DbContextOptions<XFrameworkDbContext> options)
        : base(options)
    {
    }

	public XFrameworkDbContext(DbContextOptions options) : base(options)
	{
	}

	protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(XFrameworkDbContext).Assembly);
    }
    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
}