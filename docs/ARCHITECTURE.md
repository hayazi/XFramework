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

### Metrics (OutboxDiagnostics.Meter: `XFramework.Outbox`)
All metrics use low-cardinality labels only. Never tag with EventId, aggregate IDs, or user identifiers.

**Counters:**
| Metric | Unit | Description |
|---|---|---|
| `xframework.outbox.published` | `{event}` | Events successfully published to the event bus |
| `xframework.outbox.completed` | `{message}` | Messages successfully marked completed |
| `xframework.outbox.retried` | `{message}` | Messages scheduled for retry |
| `xframework.outbox.failed` | `{message}` | Messages moved to terminal failed state |
| `xframework.outbox.lease_renewal_lost` | `{message}` | Lease renewal failed due to lost ownership |
| `xframework.outbox.completion_ownership_lost` | `{message}` | Publish succeeded but completion ownership lost |
| `xframework.outbox.retry_ownership_lost` | `{message}` | Retry ownership lost after publish failure |
| `xframework.outbox.failure_ownership_lost` | `{message}` | Failure ownership lost after publish failure |
| `xframework.outbox.lease_expired_recovered` | `{message}` | Expired processing leases recovered and re-claimed |

**Histogram:**
| Metric | Unit | Description |
|---|---|---|
| `xframework.outbox.process_duration` | `s` | Duration of outbox message processing including publish and completion |

**Observable Gauge:**
| Metric | Unit | Description |
|---|---|---|
| `xframework.outbox.pending_messages` | `{message}` | Current number of pending outbox messages awaiting processing |

No exporter wiring is included; the host configures OpenTelemetry composition.

### Metrics cardinality
Only low-cardinality labels: `outcome` (published/completed/retried/failed), `module` (event type if controlled). Never `EventId`, aggregate IDs, or user identifiers.

## Outbox Admin API

### Admin Service (EntityFrameworkCore)
- `IOutboxAdminService` (Application.Contracts.Outbox) — public admin interface
- `OutboxAdminService` (EntityFrameworkCore.Outbox) — implementation using `IOutboxRepository` and `IUnitOfWork`

**Operations:**
| Method | Description |
|---|---|
| `GetPendingAsync(page, pageSize)` | Paginated pending messages |
| `GetFailedAsync(page, pageSize)` | Paginated failed messages |
| `GetProcessingAsync(page, pageSize)` | Paginated stuck/processing messages |
| `GetByIdAsync(id)` | Full message detail (payload, headers, error, timestamps) |
| `RetryAsync(id)` | Manual retry — resets to Pending via existing `MarkRetryAsync` path (idempotent) |
| `ForceCompleteAsync(id)` | Force complete — marks Completed via existing `MarkCompletedAsync` path (idempotent, uses admin lock if no owner) |
| `GetStatsAsync()` | Counts by status (Pending, Processing, Completed, Failed) |

**Safety:**
- `RetryAsync` only allowed on `Failed` messages; uses existing ownership-fenced `MarkRetryAsync`
- `ForceCompleteAsync` works on any non-Completed status; uses `MarkCompletedAsync` with admin lock fallback
- Both operations run in a transaction with audit logging

### API Endpoints (Blazor)
All endpoints under `/api/outbox`, require authorization:
| Endpoint | Method | Description |
|---|---|---|
| `/stats` | GET | Queue statistics |
| `/pending` | GET | Pending messages (paginated) |
| `/failed` | GET | Failed messages (paginated) |
| `/processing` | GET | Processing messages (paginated) |
| `/{id:guid}` | GET | Message detail |
| `/{id:guid}/retry` | POST | Manual retry |
| `/{id:guid}/force-complete` | POST | Force complete |

### Replay Semantics
- **Retry** re-queues via existing retry path: `RetryCount++`, `NextAttemptOnUtc=now`, `Status=Pending`, lock cleared. Downstream idempotency protects against duplicate processing.
- **Force Complete** marks `Completed` with `ProcessedOnUtc=now`, lock cleared. Idempotent — safe to call multiple times.

## UI and domain conventions
- Persian/Jalali display may be used at UI boundaries; persist/operate on canonical dates (typically UTC or Gregorian) in the domain unless a specific business calendar rule requires otherwise.
- Keep localization and presentation concerns out of Domain.
- Mobile usability and multilingual support are project goals; confirm exact language set and design-system decisions before implementing.

## Security and Audit

### Authorization
- Permission-based model via `IPermissionChecker` with `AuthorizeAttribute` on services/methods.
- Interceptor pipeline enforces permissions before service execution.
- `AuthorizationApplicationServiceInterceptor` logs permission denials via `SecurityEventLogger`.

### Current User
- `ICurrentUser` abstraction with `HttpCurrentUser` implementation (claims-based).
- UserId, UserName, Roles, IsInRole available.

### Validation
- FluentValidation via `ValidationApplicationServiceInterceptor`.
- `[Validate]` attribute on methods/services.
- Validation failures logged via `SecurityEventLogger`.

### Audit
- `IAuditStore` abstraction with `EfCoreAuditStore` (EF Core) implementation.
- `AuditApplicationServiceInterceptor` auto-audits non-read-only app service calls.
- Captures: UserId (masked), UserName (masked), EntityName, EntityId, Action, Timestamp, ChangesJson (input args + result, sensitive fields masked).
- Skips methods marked `[ReadOnly]`.

### Security Event Logging
- `SecurityEventLogger` (Application.Contracts.Security) extension methods for `ILogger`:
  - `LogAuthenticationFailure` — failed login attempts
  - `LogPermissionDenied` — authorization failures
  - `LogValidationFailure` — validation errors
  - `LogAuthorizationException` — explicit auth exceptions
  - `LogSuspiciousActivity` — custom security events
- All methods mask PII: UserId (GUID → 8 chars + *** + 8 chars), UserName (email → local***@domain, plain → prefix***suffix).

### Secrets
- `ISecretProvider` abstraction with `ConfigurationSecretProvider` (IConfiguration-backed).
- Dev: user secrets / env vars via configuration.
- Prod: swap implementation for Azure Key Vault, HashiCorp Vault, etc.

### Security Headers (Blazor)
Middleware adds:
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `X-XSS-Protection: 1; mode=block`
- `Referrer-Policy: strict-origin-when-cross-origin`
- `Content-Security-Policy: default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self'; frame-ancestors 'none';`

### Trace Correlation
- `TraceId` and `SpanId` in logging scopes (LoggingApplicationServiceInterceptor).
- `ErrorInfo.TraceId` for error responses.
- Outbox trace propagation (R1): `traceparent`/`tracestate` headers through RabbitMQ.

## ERP Domain Foundation (R5 Phase 1)

### SharedKernel (XFramework.Domain.SharedKernel)
Provides reusable value objects and enums for all domain modules:

**Value Objects** (all `readonly record struct` for value semantics):
- `Money(decimal Amount, Currency Currency)` — monetary amounts with currency, arithmetic operators (+, -, *, /), comparison operators, currency-safe operations
- `Quantity(decimal Value, UnitOfMeasure Unit)` — measured quantities with unit, unit-aware arithmetic, division by decimal or same-unit quantity
- `Percentage(decimal Value)` — 0-100 range, Of operators for Money and Quantity, arithmetic operators
- `DateRange(DateTime Start, DateTime End)` — inclusive date range, Contains, Overlaps, DurationDays
- `Address(string Street, City, State, PostalCode, Country)` — formatted address, FullAddress, IsEmpty, default Country="Iran"
- `ContactInfo(string Name, Phone, Email, Address?)` — contact details with HasPhone/HasEmail/HasAddress

**Enums:**
- `Currency`: IRR, USD, EUR, GBP, AED
- `UnitOfMeasure`: Piece, Kilogram, Gram, Liter, Milliliter, Meter, Centimeter, SquareMeter, CubicMeter, Pack, Box, Roll
- `PartyType`: Customer, Supplier, Employee, Prospect, Carrier, Bank
- `DocumentStatus`: Draft, Submitted, Approved, Rejected, Cancelled, Posted
- `PostingStatus`: Unposted, Posted, Reversed
- `FiscalPeriodStatus`: Open, Closed, Locked

### Parties Module (XFramework.Domain.Parties)
Manages business parties (customers, suppliers, employees, etc.):

- **Party** (AggregateRoot<Guid>): Code, Name, TaxId, NationalId, Contact (ContactInfo), IsActive, Roles
- **PartyRoleAssignment** (Entity<Guid>): PartyId, PartyRole, ValidFrom, ValidTo?, IsActive (computed from dates)
- **PartyRole** enum: Customer, Supplier, Employee, Prospect, Carrier, Bank
- **Factory**: `Party.Create(code, name, contact, taxId?, nationalId?)`
- **Methods**: `UpdateDetails`, `AssignRole(role, validFrom?, validTo?)`, `RemoveRole(role)`, `HasRole(role)`, `Activate()`, `Deactivate()`
- **Domain Events**: PartyCreated, PartyUpdated, PartyRoleAssigned, PartyRoleRemoved, PartyActivated, PartyDeactivated

### Accounting Module (XFramework.Domain.Accounting)
Double-entry accounting with full audit trail:

- **Account** (AggregateRoot<Guid>): Code, Name, Description, AccountType (Asset/Liability/Equity/Revenue/Expense), AccountNature (Debit/Credit derived from type), Currency, ParentAccountId/ParentAccount/Children, IsActive, IsDetail
  - Nature is derived: Asset/Expense = Debit; Liability/Equity/Revenue = Credit
  - `GetBalance(IEnumerable<JournalLine>)` computes balance from journal lines
  - Hierarchy support with circular reference protection
- **JournalEntry** (AggregateRoot<Guid>): Reference, Date, Description, PartyId, Lines, Status (DocumentStatus), PostingStatus, TotalDebit, TotalCredit, IsBalanced
  - Lines: JournalLine (AccountId, Side, Amount, Description, DimensionValueId)
  - Status workflow: Draft → Submitted → Approved → Posted (or Rejected/Cancelled)
  - Double-entry validation: TotalDebit == TotalCredit required for Submit/Approve/Post
  - Posted entries can be Reversed (PostingStatus = Reversed)
- **FiscalPeriod** (AggregateRoot<Guid>): Year, PeriodNumber, Name, DateRange, Status (FiscalPeriodStatus)
  - State transitions: Open → Closed → Reopened; Open → Locked → Unlocked
  - `Contains(DateTime)` checks if date falls within period
- **Domain Events**: AccountCreated/Updated/Activated/Deactivated, JournalEntryCreated/Submitted/Approved/Rejected/Posted/Reversed/Cancelled, FiscalPeriodCreated/Closed/Reopened/Locked/Unlocked

### Design Principles
- Domain layer has no infrastructure dependencies (no EF Core, no RabbitMQ)
- Value objects use `readonly record struct` for performance and value semantics
- All state transitions emit domain events for outbox publication
- Business rules enforced in domain (not database triggers/stored procedures)
- Inventory/costing kept separate from accounting (event-driven integration planned)

## ERP Domain Extensions (R5 Phase 2)

### Inventory Module (XFramework.Domain.Inventory)
Inventory management with costing engine:

- **Item** (AggregateRoot<Guid>): Code, Name, Description, Type (Product/Service/RawMaterial/Consumable/FixedAsset/Kit), Status (Active/Inactive/Discontinued/Blocked), BaseUnit, CostingMethod (Standard/Average/Fifo/Lifo/Specific), StandardCost, IsStocked/IsPurchasable/IsSellable/IsProducible, Min/Max/Reorder levels, DefaultWarehouseId, Barcode
  - Factory: `Item.Create(...)`; Methods: `UpdateDetails`, `Activate`, `Deactivate`, `Discontinue`, `Block`, `Unblock`
- **Warehouse** (AggregateRoot<Guid>): Code, Name, Description, Type (Main/Transit/Quarantine/Scrap/Virtual), Address, IsActive, AllowNegativeStock, ManagerId
  - Factory: `Warehouse.Create(...)`; Methods: `UpdateDetails`, `Activate`, `Deactivate`
- **KardexEntry** (AggregateRoot<Guid>): ItemId, WarehouseId, TransactionType (Receipt/Issue/Transfer/Adjustment/Return/Production/Consumption), DocumentReference, TransactionDate, QuantityIn/QuantityOut, RunningBalance, UnitCost, TotalCost, CostingMethod, ReferenceDocumentId/Line
  - Factories: `CreateReceipt`, `CreateIssue`, `CreateAdjustment` — all validate stock levels
- **CostingEngine** (static): `CalculateAverageCost`, `CalculateFifoCost`, `CalculateLifoCost`, `CalculateStandardCost`, `CalculateSpecificCost`
- **Enums**: ItemType, ItemStatus, CostingMethod, InventoryTransactionType, WarehouseType
- **Domain Events**: ItemCreated/Updated/Activated/Deactivated/Discontinued/Blocked/Unblocked, WarehouseCreated/Updated/Activated/Deactivated, KardexEntryCreated

### Dimensions Module (XFramework.Domain.Dimensions)
Analytic dimensions for cost allocation and reporting:

- **CostCenter** (AggregateRoot<Guid>): Code, Name, Description, Status (Active/Inactive/Closed), ParentCostCenterId/Children hierarchy, ManagerId, BudgetAmount/BudgetCurrency
  - Factory: `CostCenter.Create(...)`; Methods: `UpdateDetails`, `Activate`, `Deactivate`, `Close`, `AddChild`
- **Project** (AggregateRoot<Guid>): Code, Name, Description, Status, StartDate, EndDate, ActualEndDate, Budget, ManagerId, CustomerId
  - Factory: `Project.Create(...)`; Methods: `UpdateDetails`, `Activate`, `Deactivate`, `Close`, `IsActiveOn(DateTime)`
- **CustomDimension** (AggregateRoot<Guid>): Code, Name, Description, DimensionKey, Status, IsRequired, AllowHierarchy, ParentDimensionId/Children, Attributes (Dictionary<string,string>)
  - Factory: `CustomDimension.Create(...)`; Methods: `UpdateDetails`, `Activate`, `Deactivate`, `SetAttribute`, `RemoveAttribute`
- **Enums**: DimensionType (CostCenter, Project, Department, Region, ProductLine, Custom), DimensionStatus (Active, Inactive, Closed)
- **Domain Events**: All state transitions for each dimension type

### Numbering Module (XFramework.Domain.Numbering)
Document numbering sequences with scoping:

- **NumberSequence** (AggregateRoot<Guid>): Code, Name, Prefix, Suffix, CurrentNumber, MinimumDigits, IncrementBy, MaximumNumber, Scope (Company/Branch/Warehouse/User/Global), ScopeIdentifier, Status (Active/Inactive/Exhausted), ResetDate, AutoReset, FormatTemplate
  - Factory: `NumberSequence.Create(...)`; Methods: `GetNextNumber`, `PeekNextNumber`, `Reset`, `UpdateDetails`, `Activate`, `Deactivate`, `SetScope`
  - Auto-reset on annual boundary, template formatting with `{PREFIX}{NUMBER}{SUFFIX}{SCOPE}` placeholders
- **Enums**: NumberingScope, NumberingStatus
- **Domain Events**: NumberSequenceCreated/Updated/Activated/Deactivated/NumberGenerated/Reset/Exhausted

### Tax Module (XFramework.Domain.Tax)
Tax/VAT handling with flexible calculation:

- **TaxCode** (AggregateRoot<Guid>): Code, Name, Description, TaxType (VAT/SalesTax/WithholdingTax/ExciseTax/CustomDuty/Other), CalculationMethod (Percentage/FixedAmount/Tiered/Custom), Application (OnNetAmount/OnGrossAmount/OnQuantity), Rate (Percentage), FixedAmount/PerUnit, Status (Active/Inactive/Expired), EffectiveFrom/To, IsDefault, IsRecoverable, AccountId, Tiers
  - Factory: `TaxCode.Create(...)`; Methods: `UpdateDetails`, `Activate`, `Deactivate`, `CalculateTax(Money, Quantity?)`
  - `CalculateTax` dispatches to appropriate method: Percentage, FixedAmount, Tiered (via TaxTier list)
- **TaxTier** (value object): ThresholdFrom, ThresholdTo, Rate — for progressive tax brackets
- **Enums**: TaxType, TaxCalculationMethod, TaxApplication, TaxStatus
- **Domain Events**: TaxCodeCreated/Updated/Activated/Deactivated

## ERP EF Core Integration (R6)

### Entity Configurations
All new domain entities have `IEntityTypeConfiguration` implementations in `XFramework.EntityFrameworkCore.Configurations/`:
- **Inventory**: ItemConfiguration, WarehouseConfiguration, KardexEntryConfiguration
- **Dimensions**: CostCenterConfiguration, ProjectConfiguration, CustomDimensionConfiguration
- **Numbering**: NumberSequenceConfiguration
- **Tax**: TaxCodeConfiguration

Configurations include proper indexing, unique constraints, foreign keys, and value object mapping via JSON serialization.

### DbContext Extensions
`XFrameworkDbContext` extended with DbSets for all new entities:
```csharp
public DbSet<Item> Items => Set<Item>();
public DbSet<Warehouse> Warehouses => Set<Warehouse>();
public DbSet<KardexEntry> KardexEntries => Set<KardexEntry>();
public DbSet<CostCenter> CostCenters => Set<CostCenter>();
public DbSet<Project> Projects => Set<Project>();
public DbSet<CustomDimension> CustomDimensions => Set<CustomDimension>();
public DbSet<NumberSequence> NumberSequences => Set<NumberSequence>();
public DbSet<TaxCode> TaxCodes => Set<TaxCode>();
```

Uses `modelBuilder.ApplyConfigurationsFromAssembly(typeof(XFrameworkDbContext).Assembly)` for automatic configuration discovery.

### Value Converters
Dedicated `ValueConverter` classes in `XFramework.EntityFrameworkCore.ValueConverters/` using JSON serialization for readonly record struct value objects:
- `MoneyConverter`, `MoneyNullableConverter` — for Money
- `QuantityConverter` — for Quantity
- `AddressConverter` — for Address
- `PercentageConverter` — for Percentage

### Repository Extensions
`IRepository` interface extended with:
- `FirstOrDefaultAsync(IQueryable<TEntity>, CancellationToken)`
- `SingleOrDefaultAsync(IQueryable<TEntity>, CancellationToken)`

Implemented in `EfCoreRepository` and `EfCoreRoleRepository` for clean Application layer queries without EF Core dependency.
