# Current Project State

## Snapshot
- Version label: V53 source baseline (R4 Framework security and audit review completed).
- Main engineering focus: Outbox reliability and observability.
- V48 confirmed by user. V49 was documentation-only. V50 adds trace propagation. V51 adds metrics. V52 adds admin API. V53 adds security & audit.

## Known established behavior
- Outbox messages are claimed in batches with lock/lease ownership.
- Expired leases can be reclaimed.
- State transitions are ownership-fenced.
- Transition methods return `bool` for success/failure.
- Processor handles lost ownership explicitly for completion/retry/failure paths and emits diagnostics.
- At-least-once delivery; consumer idempotency is mandatory.
- **Trace propagation**: OutboxProcessor starts Producer Activity (`outbox.publish`) with messaging tags; EventEnvelope carries TraceParent/TraceState/CorrelationId/CausationId; RabbitMqEventBus injects traceparent/tracestate/correlation-id/causation-id headers; RabbitMqMessageHandler extracts trace context, starts Consumer Activity (`event.process`), adds logging scope.
- **Metrics**: 9 counters (published, completed, retried, failed, lease_renewal_lost, completion_ownership_lost, retry_ownership_lost, failure_ownership_lost, lease_expired_recovered), 1 histogram (process_duration), 1 gauge (pending_messages); all low-cardinality; no exporter wiring.
- **Admin API**: IOutboxAdminService with 7 operations (GetPending, GetFailed, GetProcessing, GetById, Retry, ForceComplete, GetStats); 7 Blazor endpoints under `/api/outbox`; safe retry/force-complete using existing ownership-fenced repository methods; idempotent.
- **Security & Audit**: IAuditStore + EfCoreAuditStore; AuditApplicationServiceInterceptor auto-audits non-read-only calls; SecurityEventLogger with PII masking; ISecretProvider abstraction; security headers; TraceId/SpanId in logging scopes.

## Immediate next task
R5 — ERP domain foundation:
- Establish modules and shared kernel for Accounting, Inventory, CRM, Parties, and analytic dimensions.
- Keep inventory Kardex and costing independent from accounting posting.

## Validation commands
```powershell
dotnet test .\tests\XFramework.Tests\
dotnet test .\tests\XFramework.IntegrationTests\
dotnet build .\XFramework.slnx
```
Confirm actual solution path first. For SQL Server tests, set `XFRAMEWORK_SQLSERVER_TEST_CONNECTION` and verify test output proves the database scenario ran.
