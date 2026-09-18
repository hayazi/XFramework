# Race-Safe Idempotency

## Problem

A check-then-insert implementation such as:

```text
AnyAsync(...) -> false
Add(...) -> ProcessedMessage
```

is vulnerable when two RabbitMQ consumers process the same event concurrently. Both consumers can observe that the key does not exist before either inserts it.

## XFramework implementation

`IdempotencyService` uses a single SQL Server `INSERT ... SELECT ... WHERE NOT EXISTS` statement with:

- `UPDLOCK`
- `HOLDLOCK`
- the composite primary key `(EventId, HandlerName)`

The operation returns `true` only when this consumer inserted the idempotency record. A concurrent duplicate returns `false`.

## Transaction boundary

The idempotency insert is executed through the same `XFrameworkDbContext` used by `EventProcessor` and therefore participates in the transaction opened by `IUnitOfWork`.

```text
RabbitMQ message
      |
      v
EventProcessor
      |
      +---- BeginTransaction
      |
      +---- TryBeginProcessingAsync
      |          |
      |          +-- first -> insert + continue
      |          +-- duplicate -> skip
      |
      +---- Event Handler
      |
      +---- SaveChanges
      |
      +---- Commit
```

If the handler fails, the transaction rolls back and the idempotency record is not retained. The event can therefore be retried safely.

## Stable handler identity

`EventProcessor` now uses the closed handler contract type (for example `IEventHandler<InventoryDocumentPostedEvent>`) rather than the runtime implementation/proxy type. This prevents decorators or proxy generation from changing the idempotency key.

## Required database constraint

`ProcessedMessages` must have this primary key:

```sql
PRIMARY KEY (EventId, HandlerName)
```

This is both a data-integrity guarantee and the final protection against duplicate inserts.
