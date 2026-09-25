# Current Project State

## Snapshot
- Version label: V50 source baseline (R1 Trace/Correlation propagation completed).
- Main engineering focus: Outbox reliability and observability.
- V48 confirmed by user. V49 was documentation-only. V50 adds trace propagation with tests.

## Known established behavior
- Outbox messages are claimed in batches with lock/lease ownership.
- Expired leases can be reclaimed.
- State transitions are ownership-fenced.
- Transition methods return `bool` for success/failure.
- Processor handles lost ownership explicitly for completion/retry/failure paths and emits diagnostics.
- At-least-once delivery; consumer idempotency is mandatory.
- **Trace propagation**: OutboxProcessor starts Producer Activity (`outbox.publish`) with messaging tags; EventEnvelope carries TraceParent/TraceState/CorrelationId/CausationId; RabbitMqEventBus injects traceparent/tracestate/correlation-id/causation-id headers; RabbitMqMessageHandler extracts trace context, starts Consumer Activity (`event.process`), adds logging scope.

## Immediate next task
R2 — Outbox metrics and dashboard readiness:
- Validate existing meter/counter names and instrumentation.
- Use low-cardinality tags only (outcome, module, event type if controlled). Never tag with EventId or entity/user IDs.
- Add exporter wiring only after deciding the host's OpenTelemetry composition.

## Validation commands
```powershell
dotnet test .\tests\XFramework.Tests\
dotnet test .\tests\XFramework.IntegrationTests\
dotnet build .\XFramework.slnx
```
Confirm actual solution path first. For SQL Server tests, set `XFRAMEWORK_SQLSERVER_TEST_CONNECTION` and verify test output proves the database scenario ran.
