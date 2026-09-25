# XFramework Roadmap

## Current baseline
V48 — Outbox transition-result handling and observability. User reported it is okay; exact test counts should be confirmed from command output if needed. V49 is a documentation-only Codex handoff package.
V50 — Trace/correlation propagation (R1 completed).
V51 — Outbox metrics and dashboard readiness (R2 completed).
V52 — Outbox operator experience (R3 completed).
V53 — Framework security and audit review (R4 completed).

## Completed work (high level)
- Layered framework structure, application services and interceptor pipeline.
- Authorization, validation, UoW, localization and identity groundwork.
- Domain events, event envelope/type registry, transactional Outbox and background processing.
- RabbitMQ event bus, retry/DLQ, message processing and idempotency.
- Integration coverage for crash windows, retry/DLQ failures, atomic Outbox behavior, lease recovery/fencing, stale-worker races, and publish ownership scenarios.
- V46–V48: state transition result semantics, duplicate publish behavior, lost-ownership handling and diagnostics.
- V50 (R1): Trace/correlation propagation through Outbox → EventEnvelope → RabbitMQ headers → Consumer Activity/log scope.
- V51 (R2): Outbox metrics with 9 counters, 1 histogram, 1 gauge; all low-cardinality; 35 unit tests pass.
- V52 (R3): Outbox Admin API with 7 operations, 7 API endpoints, 14 unit tests; safe retry/force-complete with idempotency; all tests pass (32 integration, 49 unit).
- V53 (R4): Security & Audit with auto-auditing interceptor, PII-masked security event logging, secret provider abstraction, security headers, trace correlation; 60 unit tests pass.

## Recommended next work packages
Each item requires source inspection, explicit acceptance criteria, tests, and a complete source ZIP.

### R1 — Trace/correlation propagation ✅ COMPLETED (V50)
- OutboxProcessor starts Producer Activity (`outbox.publish`) with messaging tags
- EventEnvelope carries TraceParent, TraceState, CorrelationId, CausationId
- RabbitMqEventBus injects traceparent, tracestate, correlation-id, causation-id headers
- RabbitMqMessageHandler extracts trace context, starts Consumer Activity (`event.process`), adds logging scope
- Unit tests for header propagation (19 tests pass)
- All existing tests pass (32 integration, 10 unit)

### R2 — Outbox metrics and dashboard readiness ✅ COMPLETED (V51)
- 9 counters: published, completed, retried, failed, lease_renewal_lost, completion_ownership_lost, retry_ownership_lost, failure_ownership_lost, lease_expired_recovered
- 1 histogram: process_duration (seconds)
- 1 observable gauge: pending_messages
- All metrics use low-cardinality labels only; no EventId/aggregate/user tags
- No exporter wiring (host configures OpenTelemetry)
- Unit tests for metric names, units, descriptions (35 tests total)
- All existing tests pass (32 integration, 35 unit)

### R3 — Outbox operator experience ✅ COMPLETED (V52)
- IOutboxAdminService in Application.Contracts with 7 operations
- OutboxAdminService in EntityFrameworkCore with ownership-fenced retry/force-complete
- IOutboxRepository extended with 5 query methods (GetPending, GetFailed, GetProcessing, GetById, GetCountByStatus)
- 7 Blazor API endpoints under `/api/outbox` with authorization
- 14 unit tests for admin service (retry, force-complete, stats, pagination)
- Idempotency preserved: Retry uses MarkRetryAsync path; ForceComplete uses MarkCompletedAsync path
- All tests pass (32 integration, 49 unit)

### R4 — Framework security and audit review ✅ COMPLETED (V53)
- IAuditStore + EfCoreAuditStore for persistent audit trail
- AuditApplicationServiceInterceptor auto-audits non-read-only app service calls
- SecurityEventLogger with PII-masked logging for auth failures, permission denials, validation errors
- ISecretProvider abstraction with ConfigurationSecretProvider (config-backed)
- Security headers middleware in Blazor (CSP, X-Frame-Options, etc.)
- Trace correlation: TraceId/SpanId in logging scopes, ErrorInfo.TraceId, Outbox trace propagation
- 11 new unit tests for security/audit; all tests pass (32 integration, 60 unit)

### R5 — ERP domain foundation
- Establish modules and shared kernel for Accounting, Inventory, CRM, Parties, and analytic dimensions based on the documented legacy analysis.
- Keep inventory Kardex and costing independent from accounting posting.

## Definition of done for each milestone
1. Scope and acceptance criteria documented.
2. Existing behavior inspected; no duplicate abstractions.
3. Unit/integration tests added or updated.
4. Tests/build executed where possible; exact results reported.
5. Documentation updated.
6. Full source ZIP delivered.
7. User reviews and confirms before the next milestone.
