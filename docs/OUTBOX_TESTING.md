# Outbox Testing Guide

## Test layers
- Unit tests: retry policy, envelope/type registry, processor branching, idempotency logic with controlled fakes.
- Integration tests: SQL Server persistence/concurrency and real RabbitMQ topology/publish/consume/retry/DLQ behavior.
- Crash-window tests: verify redelivery and idempotent business effect when processing commits but broker ACK is absent.
- Lease tests: claim, expiry, reclaim, stale renewal, stale completion/retry/failure, and race outcomes.

## Environment-gated SQL Server tests
The opt-in connection string is:
```text
XFRAMEWORK_SQLSERVER_TEST_CONNECTION
```
Some historical tests used `return` when this variable was absent. That makes the test method return successfully; it is not the same as xUnit reporting a skipped test. Prefer a true skip mechanism or an explicit diagnostic and ensure CI runs the SQL Server suite.

## Commands
```powershell
dotnet test .\tests\XFramework.Tests\
dotnet test .\tests\XFramework.IntegrationTests\
```

## Report format
Record:
- exact command
- total/passed/failed/skipped
- whether SQL Server/RabbitMQ were real services or fakes
- environment-gated tests that did not run
- any warnings

## Key reliability assertions
- duplicate delivery does not duplicate business effects
- stale owner cannot renew or transition a reclaimed row
- retry/DLQ publish failures do not silently lose the original
- cancellation propagates rather than being converted to retry/failure
- successful publish with lost ownership is not locally republished by the same worker
