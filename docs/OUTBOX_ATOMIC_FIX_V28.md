# XFramework Outbox Atomic Persistence — v28

## Why v27 was still failing

The v27 tests showed that the `SaveChangesInterceptor` path was not reliably producing the Outbox row in the integration test DbContext. The failure-path test also relied on the duplicate EventId being detected by the interceptor, but that duplicate was being silently ignored.

## v28 architecture

Domain-event-to-outbox persistence is now owned by `DomainEventDbContext.SaveChanges` / `SaveChangesAsync`.

```text
Application changes
      |
      v
Domain Events on tracked entities
      |
      v
DomainEventDbContext.SaveChanges
      |
      +--> create OutboxMessage rows
      |
      v
base.SaveChanges
      |
      v
same EF transaction
      |
      +--> success: clear domain events
      |
      +--> failure: keep domain events
```

`DomainEventToOutboxInterceptor` remains only as a compatibility type and is no longer registered.

The Outbox EventId unique constraint is intentionally allowed to surface duplicate EventIds instead of silently suppressing manually-added duplicate Outbox rows.
