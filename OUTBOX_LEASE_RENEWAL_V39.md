# Outbox Lease Renewal Reliability — V39

## Why this change

An outbox worker claims a message with a finite lease. If publishing takes longer than the lease, another worker may legally reclaim the message while the original worker is still processing it. That creates an avoidable duplicate-publish window.

V39 renews the lease immediately before publishing each claimed message.

## Production changes

`IOutboxRepository` now exposes:

```csharp
Task<bool> RenewLeaseAsync(
    Guid messageId,
    string lockId,
    DateTime lockedUntilUtc,
    CancellationToken cancellationToken = default);
```

`OutboxRepository` renews only when the row still belongs to the same `LockId` and remains `Processing`.

`OutboxProcessor`:

1. claims the batch;
2. renews the lease for each message immediately before publish;
3. skips the message if the lease was lost;
4. publishes only while it still owns the lease.

## Tests

Added:

- `Processor_should_renew_the_lease_before_publishing`
- `Processor_should_not_publish_when_lease_was_lost`

Existing tests were updated for the new repository contract.

The user must run the test suite on the real development environment; this environment does not run the user's .NET/RabbitMQ/SQL Server stack.
