using Microsoft.EntityFrameworkCore;
using XFramework.Application.Events;
using XFramework.Domain.Auditing;
using XFramework.Domain.Dimensions;
using XFramework.Domain.Inventory;
using XFramework.Domain.Numbering;
using XFramework.Domain.Tax;
using XFramework.EntityFrameworkCore.Idempotency;
using XFramework.EntityFrameworkCore.Outbox;

namespace XFramework.EntityFrameworkCore.Persistence;

public sealed class XFrameworkDbContext(
    DbContextOptions<XFrameworkDbContext> options,
    IEventTypeRegistry eventTypeRegistry)
    : DomainEventDbContext(options, eventTypeRegistry)
{
    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    // Inventory
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<KardexEntry> KardexEntries => Set<KardexEntry>();

    // Dimensions
    public DbSet<CostCenter> CostCenters => Set<CostCenter>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<CustomDimension> CustomDimensions => Set<CustomDimension>();

    // Numbering
    public DbSet<NumberSequence> NumberSequences => Set<NumberSequence>();

    // Tax
    public DbSet<TaxCode> TaxCodes => Set<TaxCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(XFrameworkDbContext).Assembly);
    }
}
