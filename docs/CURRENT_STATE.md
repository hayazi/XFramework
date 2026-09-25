# Current Project State

## Snapshot
- Version label: V48 source baseline; V49 is documentation/context packaging.
- Main engineering focus: Outbox reliability and observability.
- User confirmed V48 is okay. Exact test counts were not captured in the confirmation.
- Do not treat this documentation-only ZIP as evidence of a new build or test run.

## Known established behavior
- Outbox messages are claimed in batches with lock/lease ownership.
- Expired leases can be reclaimed.
- State transitions are ownership-fenced.
- Transition methods return `bool` for success/failure.
- Processor handles lost ownership explicitly for completion/retry/failure paths and emits diagnostics.
- At-least-once delivery; consumer idempotency is mandatory.

## Immediate next task
Inspect V48 source before coding. Then propose a bounded tracing/correlation milestone:
- Current ActivitySource/Meter implementation
- CorrelationId/CausationId/EventId propagation
- RabbitMQ headers and consumer scope
- Test coverage and cardinality-safe metrics

## Validation commands
```powershell
dotnet test .\tests\XFramework.Tests\
dotnet test .\tests\XFramework.IntegrationTests\
dotnet build .\XFramework.slnx
```
Confirm actual solution path first. For SQL Server tests, set `XFRAMEWORK_SQLSERVER_TEST_CONNECTION` and verify test output proves the database scenario ran.
