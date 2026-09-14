using Microsoft.EntityFrameworkCore;
using XFramework.EntityFrameworkCore.Outbox;

namespace XFramework.EntityFrameworkCore;

public class XFrameworkDbContext : DbContext
{
    public XFrameworkDbContext(
        DbContextOptions<XFrameworkDbContext> options)
        : base(options)
    {
    }

    public DbSet<OutboxMessage> OutboxMessages =>
        Set<OutboxMessage>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(XFrameworkDbContext).Assembly);
    }
}