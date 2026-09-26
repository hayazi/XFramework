# Outbox SaveChanges Integration

The v33 implementation uses an EF Core `SaveChangesInterceptor` registered by `DomainEventDbContext`.
It handles both `SaveChanges` and `SaveChangesAsync`, creates OutboxMessage rows in the same DbContext operation, and clears domain events only after successful persistence.

EF Core 10 documents `ISaveChangesInterceptor` as the interception point for both SaveChanges methods.
