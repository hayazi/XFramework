# XFramework — Agent Instructions

## Mission
Maintain and evolve XFramework, a reusable modular ERP application framework for RGRE.ERP. Preserve established architecture and behavior; do not redesign casually.

## Project baseline
- Target framework: .NET 10 (`net10.0`)
- UI: Blazor Server / Interactive Server
- Persistence: EF Core + SQL Server
- Architecture: DDD and layered modular architecture
- ABP.IO: explicitly not used
- Business rules belong in the application/domain layers, not stored procedures or database triggers.

## Solution projects
- `src/XFramework.Domain`
- `src/XFramework.Application.Contracts`
- `src/XFramework.Application`
- `src/XFramework.EntityFrameworkCore`
- `src/XFramework.Infrastructure`
- `src/XFramework.Blazor`
- `tests/XFramework.Tests`
- `tests/XFramework.IntegrationTests`

## Architectural boundaries
- Domain: entities, aggregates, value objects, domain services, domain events, domain exceptions. Keep framework and infrastructure dependencies out.
- Application.Contracts: public service interfaces, DTOs, paging contracts, and application-facing contracts.
- Application: use cases, app services, orchestration, abstractions, validation/authorization/UoW pipeline, event processing contracts/logic.
- EntityFrameworkCore: DbContexts, EF configurations, repository/UoW persistence, migrations, Outbox and idempotency persistence.
- Infrastructure: external/technical integrations (RabbitMQ, background workers, logging, observability, caching, external services). RabbitMQ implementation belongs here.
- Blazor: presentation and composition root; no domain business logic.

## Hard rules
1. Do not introduce ABP.IO.
2. Do not introduce multi-tenancy unless explicitly requested.
3. Do not create a second `CrudAppService`. The canonical implementation is in `XFramework.Application.Services/CrudAppService.cs` (verify actual current path before editing).
4. Keep RabbitMQ implementation under `XFramework.Infrastructure/Messaging/RabbitMQ`.
5. Keep EF persistence in `XFramework.EntityFrameworkCore`.
6. Do not move infrastructure implementations into Application or Domain.
7. Preserve existing APIs unless the task explicitly requires a breaking change. Explain breaking changes.
8. Do not claim tests/builds passed unless you actually ran them and have the output.
9. Before editing, inspect the repository, current code, tests, and relevant docs. Do not rely on version names alone.
10. After editing, run relevant tests and build when the environment supports it; report exact commands/results and any skipped tests.
11. Keep docs synchronized with behavior and API changes.
12. Prefer small, reviewable changes and show a concise diff summary.

## Outbox and messaging invariants
- The SQL Server Outbox + RabbitMQ design guarantees at-least-once delivery, not exactly-once publication.
- Duplicate publication is possible across crash/lease-expiry windows. Consumers must be idempotent.
- Idempotency key is `EventId + HandlerName`; the idempotency record and business side effects must share a transaction.
- Lease ownership must be fenced by message ID, lock ID, status, and valid lease time where applicable.
- A worker that loses ownership must not perform further state transitions as if it still owns the row.
- A publish may succeed while completion fails because ownership was lost. Do not locally republish from that completion-failure branch; a later worker may republish the same event.
- Never claim distributed exactly-once semantics across SQL Server and RabbitMQ.

## Before starting a task
1. Read `docs/CODEX_HANDOFF.md`.
2. Read `docs/ARCHITECTURE.md`, `docs/DECISIONS.md`, `docs/ROADMAP.md`, and the relevant topic docs.
3. Inspect `git status`, solution/project structure, and the exact current implementation.
4. Identify acceptance criteria and tests before coding.

## Validation
Run from repository root:
```powershell
dotnet test .\tests\XFramework.Tests\
dotnet test .\tests\XFramework.IntegrationTests\
dotnet build .\XFramework.slnx
```
Use the solution filename that actually exists if it differs. SQL Server opt-in tests require `XFRAMEWORK_SQLSERVER_TEST_CONNECTION`; verify whether those tests truly execute or merely return early when the variable is absent.

## Task completion report
Include:
- What changed and why
- Files changed
- Commands actually run and exact outcomes
- Tests not run and why
- Known risks / follow-up
- Confirmation that docs were updated
