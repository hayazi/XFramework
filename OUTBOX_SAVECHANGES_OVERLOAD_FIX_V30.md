# Outbox SaveChanges overload fix — v30

## Root cause
The previous `DomainEventDbContext` overrode only:

- `SaveChanges(bool acceptAllChangesOnSuccess)`
- `SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken)`

The integration tests call the normal application overload:

```csharp
await context.SaveChangesAsync();
```

Therefore the domain-event preparation boundary was not guaranteed to execute.
That explained both v29 failures:

- no Outbox row was created;
- the duplicate EventId did not reach the database unique constraint.

## Fix
`DomainEventDbContext` now overrides all public SaveChanges entry points:

- `SaveChanges()`
- `SaveChanges(bool)`
- `SaveChangesAsync(CancellationToken)`
- `SaveChangesAsync(bool, CancellationToken)`

The parameterless overloads delegate to the existing bool overloads, so domain events are prepared exactly once per SaveChanges operation.

No production RabbitMQ behavior was changed.
