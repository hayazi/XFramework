# Current Project State

## Snapshot
- Version label: V60 (R9 Parties & Accounting Blazor UI/API completed).
- Main engineering focus: ERP domain modules (SharedKernel, Parties, Accounting, Inventory, Dimensions, Numbering, Tax) with full EF Core persistence, Application services, Blazor UI + Minimal API endpoints.
- V48 confirmed by user. V49 was documentation-only. V50 adds trace propagation. V51 adds metrics. V52 adds admin API. V53 adds security & audit. V54 adds ERP domain foundation (R5 Phase 1). V55 adds ERP domain extensions (R5 Phase 2). V56 adds Application layer services (R5 Phase 3). V57 adds EF Core integration (R6). V58 adds Blazor UI and Minimal API endpoints (R7). V59 adds Parties & Accounting Application & Persistence (R8). V60 adds Parties & Accounting Blazor UI & API (R9).

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
- **ERP Domain Foundation (R5 Phase 1)**:
  - **SharedKernel**: Value objects (Money, Quantity, Percentage, DateRange, Address, ContactInfo) using `readonly record struct`; Enums (Currency, UnitOfMeasure, PartyType, DocumentStatus, PostingStatus, FiscalPeriodStatus).
  - **Parties Module**: Party aggregate with code/name/taxId/nationalId/contact; PartyRoleAssignment with validity periods; PartyRole enum (Customer, Supplier, Employee, Prospect, Carrier, Bank); Domain events (PartyCreated, PartyUpdated, PartyRoleAssigned, PartyRoleRemoved, PartyActivated, PartyDeactivated).
  - **Accounting Module**: Account aggregate with code/name/type/nature/currency/hierarchy; JournalEntry aggregate with lines, double-entry validation, status workflow (Draft→Submitted→Approved→Posted/Reversed/Cancelled); JournalLine value object; FiscalPeriod with Open/Closed/Locked states; Domain events for all state transitions.
- **ERP Domain Extensions (R5 Phase 2)**:
  - **Inventory Module**: Item aggregate (code, name, type, base unit, costing method, standard cost, stock flags); Warehouse aggregate (code, name, type, address, negative stock allowance); CardexEntry aggregate (receipt/issue/adjustment, running balance, cost tracking); CostingEngine (Average, FIFO, LIFO, Standard, Specific costing); Enums (ItemType, ItemStatus, CostingMethod, InventoryTransactionType, WarehouseType); Domain events for all state transitions.
  - **Dimensions Module**: CostCenter aggregate (hierarchical, budget, manager); Project aggregate (dates, budget, manager, customer); CustomDimension aggregate (flexible key-value attributes, hierarchy support); Enums (DimensionType, DimensionStatus); Domain events for all state transitions.
  - **Numbering Module**: NumberSequence aggregate (prefix/suffix, scope, auto-reset, format template, min/max); NumberingScope (Company, Branch, Warehouse, User, Global); NumberingStatus (Active, Inactive, Exhausted); Domain events for number generation, reset, exhaustion.
  - **Tax Module**: TaxCode aggregate (VAT, SalesTax, Withholding, Excise, CustomDuty); Calculation methods (Percentage, FixedAmount, Tiered, Custom); TaxApplication (OnNetAmount, OnGrossAmount, OnQuantity); TaxTier for tiered rates; IsRecoverable flag; Domain events for all state transitions.
- **ERP Application Layer Services (R5 Phase 3)**:
  - **Inventory**: IItemAppService, IWarehouseAppService, ICardexAppService with full CRUD + custom queries (GetByCode, GetByType, GetLowStock, Activate/Deactivate/Discontinue/Block/Unblock, GetCurrentStock)
  - **Dimensions**: ICostCenterAppService, IProjectAppService, ICustomDimensionAppService with full CRUD + hierarchy queries, activation/deactivation/close
  - **Numbering**: INumberSequenceAppService with CRUD + GetNextNumber, PeekNextNumber, Reset, scope-based lookup
  - **Tax**: ITaxCodeAppService with CRUD + CalculateTax, GetDefault, GetByType
  - All services use [Validate] attribute, auto-discovered via ApplicationServiceDiscovery, registered with interceptor pipeline (Logging, Authorization, Validation, UnitOfWork, Audit)
  - Repository interface extended with FirstOrDefaultAsync, SingleOrDefaultAsync for Application layer queries without EF Core dependency
- **ERP EF Core Integration (R6)**:
  - **Configurations**: All new domain entities have IEntityTypeConfiguration with proper indexing, unique constraints, and value object mapping via JSON serialization (Money, Quantity, Address, Percentage)
  - **DbContext**: XFrameworkDbContext extended with DbSets for Items, Warehouses, CardexEntries, CostCenters, Projects, CustomDimensions, NumberSequences, TaxCodes
  - **Value Converters**: Dedicated ValueConverter classes (MoneyConverter, QuantityConverter, AddressConverter, PercentageConverter, MoneyNullableConverter) using JSON serialization for readonly record struct value objects
  - **Repository**: EfCoreRepository implements new FirstOrDefaultAsync/SingleOrDefaultAsync methods for Application layer queries
- **ERP Blazor UI & API Endpoints (R7)**:
  - **Blazor Pages**: 8 interactive pages (Items, Warehouses, Cardex, CostCenters, Projects, CustomDimensions, NumberSequences, TaxCodes) with full CRUD modals, filtering, and custom actions
  - **Navigation**: Updated NavMenu with grouped links for Inventory, Dimensions, Numbering, Tax modules
  - **Minimal API Endpoints**: Program.cs extended with 4 route groups under `/api/inventory`, `/api/dimensions`, `/api/numbering`, `/api/tax` providing full CRUD + custom actions for all new modules
  - All pages use `@rendermode InteractiveServer`, Bootstrap 5 styling, proper DTO alignment with Application.Contracts
- **ERP Parties & Accounting Application & Persistence (R8)**:
  - **Parties Application Services**: IPartyAppService with full CRUD + role assignment/removal, queries by code/name/role/active status, Activate/Deactivate
  - **Accounting Application Services**: 
    - IAccountAppService with full CRUD + hierarchy queries, Activate/Deactivate, SetParent/RemoveParent
    - IJournalEntryAppService with full CRUD + workflow operations (Submit, Approve, Reject, Post, Reverse, Cancel) and queries by reference/party/date/status
    - IFiscalPeriodAppService with full CRUD + lifecycle operations (Close, Reopen, Lock, Unlock, GetCurrentPeriod)
  - **EF Core Persistence (R8)**: 
    - Party & PartyRoleAssignment configurations with JSON serialization for ContactInfo value object
    - Account, JournalEntry, FiscalPeriod configurations with JSON serialization for JournalLine list and DateRange value object
    - All configurations include proper indexes, unique constraints, foreign keys
    - XFrameworkDbContext extended with DbSets for Parties, PartyRoleAssignments, Accounts, JournalEntries, FiscalPeriods
    - Migration `R8_Parties_Accounting` applied
- **ERP Parties & Accounting Blazor UI & API (R9)**:
  - **Blazor Pages**: 5 new interactive pages (Parties, Accounts, JournalEntries, FiscalPeriods, OutboxMonitor) with RadzenDataGrid, server-side paging/sorting/filtering
  - **Navigation**: NavMenu updated with grouped links for Parties and Accounting modules, plus Administration/OutboxMonitor
  - **Minimal API Endpoints**: Program.cs extended with route groups under `/api/parties`, `/api/accounting` providing full CRUD for all new modules
  - All pages use `@rendermode InteractiveServer`, Bootstrap 5 styling, Radzen components, Persian (fa-IR) default culture with RTL support
  - **Localization**: 100+ new keys in SharedResource.en.resx and SharedResource.fa.resx for Parties, Accounting, Outbox modules
  - **Outbox Monitor**: Stats cards (Total/Pending/Failed/Processing), single DataGrid with all messages, Retry/ForceComplete actions

## Immediate next task
None - R9 complete. Ready for next phase planning (R10: Sales & Purchasing modules, or Identity & Authorization UI).

## Validation commands
```powershell
dotnet test .\tests\XFramework.Tests\
dotnet test .\tests\XFramework.IntegrationTests\
dotnet build .\XFramework.slnx
```
Confirm actual solution path first. For SQL Server tests, set `XFRAMEWORK_SQLSERVER_TEST_CONNECTION` and verify test output proves the database scenario ran.