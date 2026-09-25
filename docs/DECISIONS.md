# Architecture Decision Record — XFramework

This is a compact record of decisions made during project planning and implementation. When changing a decision, add a dated ADR rather than silently rewriting intent.

## ADR-001 — Custom framework, no ABP.IO
**Decision:** Build XFramework directly on .NET rather than depending on ABP.IO.  
**Reason:** Keep control over architecture and reusable framework behavior.

## ADR-002 — No multi-tenancy
**Decision:** Do not introduce multi-tenancy unless the user explicitly changes the requirement.

## ADR-003 — Layered DDD solution
**Decision:** Keep Domain, Application.Contracts, Application, EntityFrameworkCore, Infrastructure, and Blazor as distinct projects.  
**Reason:** Clear separation of business rules, orchestration, persistence, external systems, and UI.

## ADR-004 — Infrastructure owns external implementations
**Decision:** RabbitMQ, background services, logging/observability providers, and external integrations belong in `XFramework.Infrastructure`. EF persistence belongs in `XFramework.EntityFrameworkCore`.

## ADR-005 — Single canonical CrudAppService
**Decision:** Canonical location is `XFramework.Application.Services/CrudAppService.cs`. Do not create a duplicate implementation.

## ADR-006 — Business logic not in the database
**Decision:** Put business rules in Domain/Application, not stored procedures or triggers. SQL is allowed for persistence operations and carefully designed atomic concurrency primitives.

## ADR-007 — Outbox + at-least-once delivery
**Decision:** Persist event records transactionally with business state and publish asynchronously. Delivery is at least once; duplicates are expected under crash/reclaim windows. Exactly-once publication across SQL Server and RabbitMQ is not claimed.

## ADR-008 — Idempotent consumers
**Decision:** Deduplicate using `EventId + HandlerName`; idempotency marker and business effects must commit in the same transaction.

## ADR-009 — Lease fencing
**Decision:** Outbox state changes must be guarded by current ownership, using message ID, LockId, Processing status, and valid lease time as appropriate. A stale worker must not overwrite a reclaimed worker's state.

## ADR-010 — Lost ownership after successful publish
**Decision:** If publish succeeds but completion reports lost ownership, the old worker does not retry publication locally. A later worker may publish the same EventId; downstream idempotency protects business effects.

## ADR-011 — User delivery preference
**Decision:** For source-code changes, provide a complete updated source ZIP, not only snippets. Also include a concise summary, test commands/results, and documentation.

## ADR-012 — Verification honesty
**Decision:** Clearly distinguish tests actually executed from tests skipped or gated by environment variables. Never invent build/test results.
