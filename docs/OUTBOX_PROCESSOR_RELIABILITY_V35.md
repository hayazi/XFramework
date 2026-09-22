# Outbox Processor Reliability — v35

This version continues from the user-verified 13/13 integration-test baseline.

## Verified baseline supplied by the user

`XFramework.IntegrationTests`:
- Total: 13
- Failed: 0
- Succeeded: 13
- Skipped: 0

## v35 changes

### 1. Cancellation is no longer treated as a publish failure

`OutboxProcessor.ProcessBatchAsync` now lets cancellation propagate when the supplied cancellation token is cancelled:

```csharp
catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
{
    throw;
}
```

This prevents a graceful application shutdown from incorrectly incrementing retry state or marking an outbox message for retry.

### 2. OutboxProcessor reliability tests

Added four tests covering:

- successful publish → completed
- retryable publish failure → retry
- non-retryable publish failure → failed
- cancellation during publish → cancellation propagates and no retry/failure state is written

These tests use in-memory fakes for the repository and event bus. They do not require RabbitMQ or SQL Server.

## Next validation

Run both suites:

```powershell
dotnet test .\tests\XFramework.Tests\
dotnet test .\tests\XFramework.IntegrationTests\
```

The existing 13 integration tests should remain green, and the new OutboxProcessor tests should pass.

This document does not claim test execution in this environment.
