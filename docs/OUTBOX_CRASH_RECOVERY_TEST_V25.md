# Outbox Reliability Test v25

## Added scenarios

### 1. Atomic Outbox persistence
`OutboxAtomicPersistenceTests` verifies that a domain event and its Outbox message participate in the same database transaction:

- commit => aggregate + Outbox row are both persisted
- rollback => aggregate + Outbox row are both absent
- `DomainEventToOutboxInterceptor` is exercised, not bypassed

The test uses an in-memory SQLite database so it does not require a separate SQL Server instance.

### 2. Crash recovery / expired lease
`OutboxCrashRecoveryTests` verifies the worker recovery state machine:

```text
Processing + expired lease
        ↓
ReleaseExpiredLeases
        ↓
Pending
        ↓
ClaimBatch
        ↓
Processing + new lease
        ↓
RabbitMQ publish abstraction
        ↓
Completed
```

This models the important restart case where a worker dies after claiming an Outbox row and before completing it.

## Important scope

This version proves the atomic persistence and lease-recovery state machine deterministically. It does not claim to simulate a real OS process kill between RabbitMQ publish and `MarkCompletedAsync`. That next scenario should use the real SQL Server Outbox repository plus RabbitMQ and verify duplicate delivery/idempotency across the publish/complete crash window.

## Run

```powershell
dotnet test .\tests\XFramework.IntegrationTests\
```

Expected: 11 integration tests passed.
