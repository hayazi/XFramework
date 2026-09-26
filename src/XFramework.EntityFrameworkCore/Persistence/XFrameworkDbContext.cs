using Microsoft.EntityFrameworkCore;
using XFramework.Application.Events;
using XFramework.Domain.Auditing;
using XFramework.Domain.Accounting;
using XFramework.Domain.Dimensions;
using XFramework.Domain.Inventory;
using XFramework.Domain.Numbering;
using XFramework.Domain.Parties;
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

    // Parties
    public DbSet<Party> Parties => Set<Party>();
    public DbSet<PartyRoleAssignment> PartyRoleAssignments => Set<PartyRoleAssignment>();

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

    // Accounting
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<FiscalPeriod> FiscalPeriods => Set<FiscalPeriod>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(XFrameworkDbContext).Assembly);
    }
}
