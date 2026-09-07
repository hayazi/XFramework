using Microsoft.EntityFrameworkCore;

namespace XFramework.EntityFrameworkCore;

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
}