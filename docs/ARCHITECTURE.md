# XFramework Architecture

## Purpose and constraints
XFramework is a reusable foundation for RGRE.ERP, a modern rewrite of a legacy Delphi/SQL Server MIS. The target is .NET 10+, Blazor Server/Interactive Server, SQL Server, DDD, multilingual UI, and business logic in application/domain code. ABP.IO and multi-tenancy are not part of the current plan.

## Projects and responsibilities

| Project | Responsibility | Must not own |
|---|---|---|
| `XFramework.Domain` | Entities, aggregate roots, value objects, domain services, domain events, domain exceptions | EF Core, RabbitMQ, Blazor, HTTP/infrastructure clients |
| `XFramework.Application.Contracts` | DTOs, service interfaces, paging and public application contracts | Persistence implementation, UI |
| `XFramework.Application` | Use cases, application services, orchestration, validation/auth/UoW pipeline, event processor and abstractions | RabbitMQ client implementation, EF implementation, Blazor |
| `XFramework.EntityFrameworkCore` | DbContexts, EF configurations, repositories/UoW persistence, migrations, Outbox/idempotency persistence | UI, broker client |
| `XFramework.Infrastructure` | RabbitMQ, background workers, logging, observability, caching, email/files/external integrations | Domain business rules, Blazor UI |
| `XFramework.Blazor` | UI, auth/UI integration, localization, DI composition root and host startup | Domain business logic |
| `XFramework.Tests` | Unit tests | External-service assumptions unless mocked |
| `XFramework.IntegrationTests` | Integration and reliability tests | Silent substitution of fake tests for real infrastructure tests |

## Dependency direction
Conceptually:
```text
Domain
  ↑
Application.Contracts
  ↑
Application
  ↑                 ↑
EntityFrameworkCore  Infrastructure
  \                 /
        Blazor (composition root)
```
Check actual `.csproj` references before changing. Implementations may depend on contracts/abstractions; Domain should remain clean.

## Application pipeline
The established intended order is:
```text
Request
  -> Logging
  -> Authorization
  -> Validation
  -> UnitOfWork
  -> Application Service
```
Inspect the current interceptor registration/order before modifying.

## Canonical CrudAppService
There must be one canonical implementation. The project decision is `XFramework.Application.Services/CrudAppService.cs`. Historical conversations discussed a duplicate root-level `XFramework.Application/CrudAppService.cs`; that duplicate was not intended. Verify current source tree before editing or moving files.

## Event-driven accounting/inventory target
For RGRE.ERP, inventory documents should not synchronously create accounting vouchers as a hidden side effect. Target flow:
```text
Inventory Document
  -> Inventory Transaction / Kardex
  -> Costing Engine
  -> Domain/Integration Event
  -> Outbox
  -> Queue
  -> Accounting Posting Handler
  -> Accounting Document
```
Inventory quantities and costing should not depend on accounting voucher timing. Do not create an analytic Center per Good-Warehouse relation. Separate analytic dimensions from business parties.

## Observability and tracing

### Outbox processor (EntityFrameworkCore)
- `OutboxDiagnostics.ActivitySource` (`XFramework.Outbox`) starts a Producer activity (`outbox.publish`) for each message.
- Tags: `messaging.system=rabbitmq`, `messaging.destination=<EventType>`, `messaging.message_id=<EventId>`, `messaging.correlation_id`, `messaging.causation_id`, `messaging.message_retry_count`.
- `TraceParent` and `TraceState` injected into `EventEnvelope` for propagation.

### RabbitMQ publisher (Infrastructure)
- `RabbitMqEventBus` adds headers: `event-id`, `event-type`, `event-version`, `retry-count`, `correlation-id`, `causation-id`, `traceparent`, `tracestate`.

### Consumer (Infrastructure)
- `MessagingDiagnostics.ActivitySource` (`XFramework.Messaging`) starts a Consumer activity (`event.process`) with parent context extracted from `traceparent`/`tracestate` headers.
- Tags mirror producer tags plus `messaging.destination=<RoutingKey>`.
- Logging scope includes `EventId`, `EventType`, `CorrelationId`, `CausationId`, `TraceId`, `SpanId`.

### Correlation flow
Domain event → OutboxMessage → EventEnvelope (TraceParent/TraceState/CorrelationId/CausationId) → RabbitMQ headers → Consumer Activity/log scope.

### Metrics cardinality
Only low-cardinality labels: `outcome` (published/completed/retried/failed), `module` (event type if controlled). Never `EventId`, aggregate IDs, or user identifiers.

## UI and domain conventions
- Persian/Jalali display may be used at UI boundaries; persist/operate on canonical dates (typically UTC or Gregorian) in the domain unless a specific business calendar rule requires otherwise.
- Keep localization and presentation concerns out of Domain.
- Mobile usability and multilingual support are project goals; confirm exact language set and design-system decisions before implementing.
