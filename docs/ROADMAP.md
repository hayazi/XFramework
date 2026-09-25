# XFramework Roadmap

## Current baseline
V48 — Outbox transition-result handling and observability. User reported it is okay; exact test counts should be confirmed from command output if needed. V49 is a documentation-only Codex handoff package.
V50 — Trace/correlation propagation (R1 completed).

## Completed work (high level)
- Layered framework structure, application services and interceptor pipeline.
- Authorization, validation, UoW, localization and identity groundwork.
- Domain events, event envelope/type registry, transactional Outbox and background processing.
- RabbitMQ event bus, retry/DLQ, message processing and idempotency.
- Integration coverage for crash windows, retry/DLQ failures, atomic Outbox behavior, lease recovery/fencing, stale-worker races, and publish ownership scenarios.
- V46–V48: state transition result semantics, duplicate publish behavior, lost-ownership handling and diagnostics.
- V50 (R1): Trace/correlation propagation through Outbox → EventEnvelope → RabbitMQ headers → Consumer Activity/log scope.

## Recommended next work packages
Each item requires source inspection, explicit acceptance criteria, tests, and a complete source ZIP.

### R1 — Trace/correlation propagation ✅ COMPLETED (V50)
- OutboxProcessor starts Producer Activity (`outbox.publish`) with messaging tags
- EventEnvelope carries TraceParent, TraceState, CorrelationId, CausationId
- RabbitMqEventBus injects traceparent, tracestate, correlation-id, causation-id headers
- RabbitMqMessageHandler extracts trace context, starts Consumer Activity (`event.process`), adds logging scope
- Unit tests for header propagation (19 tests pass)
- All existing tests pass (32 integration, 10 unit)

### R2 — Outbox metrics and dashboard readiness
- Validate existing meter/counter names and instrumentation.
- Use low-cardinality tags only (e.g., outcome, module, event type only if controlled). Never tag with EventId or entity/user IDs.
- Add exporter wiring only after deciding the host's OpenTelemetry composition.

### R3 — Outbox operator experience
- Inspect current failed-message visibility/replay functionality.
- Design safe administrative replay, audit trail, and operational diagnostics if absent.
- Preserve idempotency and replay semantics.

### R4 — Framework security and audit review
- Review authorization defaults, current-user identity mapping, secret/config handling, audit events, and error exposure.

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
