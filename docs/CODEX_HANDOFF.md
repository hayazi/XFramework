# Codex Handoff — XFramework

**Baseline:** V48  
**Handoff package:** V49 — CodexReady  
**Project purpose:** reusable .NET framework for the RGRE ERP/CRM rewrite  
**Current focus:** hardening the transactional Outbox and event delivery reliability.

## Status
The user confirmed V48 is okay/green in the working conversation. Exact V48 test counts were not provided in that confirmation; do not invent them. Earlier V47 test runs were reported green by the user. This package is documentation/context preparation, not a new application-code milestone.

## What the framework is
XFramework is a custom framework (not ABP.IO) for a modular ERP/CRM solution. Target stack: .NET 10, Blazor Server/Interactive Server, EF Core, SQL Server, DDD, RabbitMQ, and cross-cutting concerns such as validation, authorization, UoW, auditing/logging, observability, and background processing.

## Current architecture
See `ARCHITECTURE.md`. The agreed projects are Domain, Application.Contracts, Application, EntityFrameworkCore, Infrastructure, and Blazor, plus unit and integration test projects.

## Current Outbox guarantees
- Transactional Outbox persists event records alongside business data in the same SQL transaction.
- A background processor claims rows using a lease/lock and publishes to the event bus.
- Retry/backoff and terminal failure/DLQ paths are present.
- Atomic claiming and lease recovery/fencing have been developed and tested in stages.
- Delivery is at least once. Duplicate delivery/publication can happen.
- Consumer idempotency uses EventId + handler identity and must be transactionally coupled to side effects.
- State-transition methods return success/failure results so lost ownership can be observed.
- V48 adds transition-result handling and diagnostics counters/logging for lost ownership paths (as described in the current V48 implementation and its notes).

## Milestone history (high-level)
- V12: integration-test isolation.
- V13–V18: idempotency, retry, DLQ, malformed message handling and related test fixes.
- V19–V22: crash-window redelivery and retry-publish-failure scenarios.
- V23–V24: DLQ publish failure behavior and test correction.
- V25–V34: Outbox atomicity and persistence lifecycle/test refinements; the user corrected key atomicity tests in their own baseline.
- V35–V40: Outbox processor reliability, cancellation handling, worker lifecycle/lease renewal groundwork.
- V41–V43: opt-in SQL Server concurrent claim and lease expiration hardening.
- V44–V45: lease fencing, reclaim, and stale worker completion races.
- V46: boolean state-transition result semantics.
- V47: duplicate-publish/lost-ownership behavior.
- V48: retry/failure ownership-result handling and diagnostics/metrics.
- V49: documentation/context handoff for Codex; no intended application behavior changes.

## Next recommended engineering step
After verifying the V48 source and tests in the user's repository, continue with a scoped observability task:
1. Inspect current V48 metrics and logs; avoid duplicate instrumentation.
2. Add ActivitySource/tracing only where useful and consistently propagated.
3. Propagate CorrelationId/CausationId/EventId through Outbox -> EventEnvelope -> RabbitMQ headers -> consumer logs/traces.
4. Define low-cardinality metric labels. Never use EventId, aggregate IDs, or user identifiers as metric labels.
5. Add tests for propagation and diagnostics.
6. Update docs and provide a complete ZIP/reviewable commit.
Do not begin automatically without inspecting current files and agreeing on acceptance criteria.

## Important unknowns / verification items
- Verify the exact project tree and canonical paths in the extracted source; prior notes contain occasional historical path variations.
- Confirm exact V48 test counts from actual user output before recording numeric totals.
- Verify SQL Server opt-in tests really ran. If a test returns early when the connection string is absent, it is not a true skip and must not be counted as meaningful database validation.
- Review whether all Outbox state transitions use consistent UTC/time-bound fencing and whether lost-ownership metrics are wired to a MeterListener/OpenTelemetry exporter.
- Confirm V48’s diagnostics implementation is in the correct layer and does not introduce unwanted dependency direction.
