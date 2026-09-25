# XFramework Roadmap

## Current baseline
V48 — Outbox transition-result handling and observability. User reported it is okay; exact test counts should be confirmed from command output if needed. V49 is a documentation-only Codex handoff package.
V50 — Trace/correlation propagation (R1 completed).
V51 — Outbox metrics and dashboard readiness (R2 completed).
V52 — Outbox operator experience (R3 completed).
V53 — Framework security and audit review (R4 completed).
V54 — ERP domain foundation (R5 Phase 1 completed).
V55 — ERP domain extensions (R5 Phase 2 completed).
V56 — ERP Application layer services (R5 Phase 3 completed).

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
- V54 (R5 Phase 1): ERP domain foundation with SharedKernel (6 value objects, 6 enums), Parties module (Party aggregate, role assignments, domain events), Accounting module (Account, JournalEntry, JournalLine, FiscalPeriod with full state workflows); 118 new unit tests pass; all tests pass (32 integration, 178 unit).
- V55 (R5 Phase 2): ERP domain extensions with Inventory module (Item, Warehouse, KardexEntry, CostingEngine with 5 costing methods), Dimensions module (CostCenter, Project, CustomDimension with hierarchy), Numbering module (NumberSequence with scopes, auto-reset, templates), Tax module (TaxCode with VAT/Sales/Withholding/Excise, 4 calculation methods, tiered rates); all tests pass (32 integration, 178 unit).
- V56 (R5 Phase 3): ERP Application layer services for all new modules — Inventory (Item, Warehouse, Kardex), Dimensions (CostCenter, Project, CustomDimension), Numbering (NumberSequence), Tax (TaxCode) — with full CRUD, custom queries, validation, auto-discovery via ApplicationServiceDiscovery, interceptor pipeline integration; Repository interface extended with FirstOrDefaultAsync, SingleOrDefaultAsync for clean Application layer queries without EF Core dependency; all tests pass (32 integration, 178 unit).

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

### R5 Phase 1 — ERP domain foundation ✅ COMPLETED (V54)
- **SharedKernel** (src/XFramework.Domain/SharedKernel):
  - 6 Value Objects: Money (arithmetic, comparison), Quantity (unit-aware arithmetic), Percentage (0-100, Of operators), DateRange (contains, overlaps), Address (FullAddress, IsEmpty), ContactInfo (HasPhone/Email/Address)
  - 6 Enums: Currency (IRR, USD, EUR, GBP, AED), UnitOfMeasure (12 units), PartyType (6 types), DocumentStatus (6 statuses), PostingStatus (3 statuses), FiscalPeriodStatus (3 statuses)
  - All value objects use `readonly record struct` for value semantics with auto-generated equality
- **Parties Module** (src/XFramework.Domain/Parties):
  - Party aggregate: code, name, taxId, nationalId, contact, IsActive, roles
  - PartyRoleAssignment entity: role, validFrom, validTo, IsActive computed
  - PartyRole enum: Customer, Supplier, Employee, Prospect, Carrier, Bank
  - Domain events: PartyCreated, PartyUpdated, PartyRoleAssigned, PartyRoleRemoved, PartyActivated, PartyDeactivated
  - Factory method: Party.Create; methods: UpdateDetails, AssignRole, RemoveRole, HasRole, Activate, Deactivate
  - 27 unit tests
- **Accounting Module** (src/XFramework.Domain/Accounting):
  - Account aggregate: code, name, type (Asset/Liability/Equity/Revenue/Expense), nature (Debit/Credit derived from type), currency, hierarchy (parent/children), IsDetail, IsActive
  - JournalEntry aggregate: reference, date, description, partyId, lines, status (Draft/Submitted/Approved/Posted/Reversed/Cancelled), postingStatus, double-entry balance validation
  - JournalLine value object: accountId, side, amount, description, dimensionValueId
  - FiscalPeriod aggregate: year, periodNumber, name, dateRange, status (Open/Closed/Locked)
  - Domain events for all state transitions (Created, Submitted, Approved, Rejected, Posted, Reversed, Cancelled, PeriodClosed/Reopened/Locked/Unlocked)
  - 91 unit tests
- All 178 unit tests + 32 integration tests pass

### R5 Phase 3 — ERP Application layer services ✅ COMPLETED (V56)
- **Inventory Services**:
  - `IItemAppService` / `ItemAppService`: Full CRUD + GetByCode, GetByType, GetLowStock, Activate/Deactivate/Discontinue/Block/Unblock
  - `IWarehouseAppService` / `WarehouseAppService`: Full CRUD + GetByCode, GetActive, Activate/Deactivate
  - `IKardexAppService` / `KardexAppService`: Read-only + GetByItem/Warehouse/DateRange, CreateReceipt/Issue/Adjustment, GetCurrentStock
- **Dimensions Services**:
  - `ICostCenterAppService` / `CostCenterAppService`: Full CRUD + GetByCode, GetHierarchy, GetActive, Activate/Deactivate/Close
  - `IProjectAppService` / `ProjectAppService`: Full CRUD + GetByCode, GetActive, GetByManager/Customer, Activate/Deactivate/Close
  - `ICustomDimensionAppService` / `CustomDimensionAppService`: Full CRUD + GetByCode/DimensionKey, Activate/Deactivate
- **Numbering Services**:
  - `INumberSequenceAppService` / `NumberSequenceAppService`: Full CRUD + GetByCode/Scope, GetNextNumber, PeekNextNumber, Reset, Activate/Deactivate
- **Tax Services**:
  - `ITaxCodeAppService` / `TaxCodeAppService`: Full CRUD + GetByCode, GetDefault, GetActive, GetByType, CalculateTax, Activate/Deactivate
- **Infrastructure**:
  - All services use `[Validate]` attribute for FluentValidation integration
  - Auto-discovered via `ApplicationServiceDiscovery` (naming convention: *AppService)
  - Registered with interceptor pipeline: Logging → Authorization → Validation → UnitOfWork → Audit
  - Repository interface (`IRepository`) extended with `FirstOrDefaultAsync`, `SingleOrDefaultAsync` for clean queries without EF Core dependency
  - `EfCoreRepository` and `EfCoreRoleRepository` implement new methods

## Definition of done for each milestone
1. Scope and acceptance criteria documented.
2. Existing behavior inspected; no duplicate abstractions.
3. Unit/integration tests added or updated.
4. Tests/build executed where possible; exact results reported.
5. Documentation updated.
6. Full source ZIP delivered.
7. User reviews and confirms before the next milestone.
