using Microsoft.EntityFrameworkCore;
using XFramework.EntityFrameworkCore.Idempotency;
using XFramework.EntityFrameworkCore.Outbox;
namespace XFramework.EntityFrameworkCore.Persistence;
public sealed class XFrameworkDbContext(DbContextOptions<XFrameworkDbContext> options):DbContext(options)
{
    public DbSet<OutboxMessage> OutboxMessages=>Set<OutboxMessage>();
    public DbSet<ProcessedMessage> ProcessedMessages=>Set<ProcessedMessage>();
    protected override void OnModelCreating(ModelBuilder modelBuilder){base.OnModelCreating(modelBuilder);modelBuilder.ApplyConfigurationsFromAssembly(typeof(XFrameworkDbContext).Assembly);}
}
