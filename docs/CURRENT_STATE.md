# Current Project State

## Snapshot
- Version label: V55 source baseline (R5 Phase 2 ERP domain extensions completed).
- Main engineering focus: ERP domain modules (SharedKernel, Parties, Accounting, Inventory, Dimensions, Numbering, Tax).
- V48 confirmed by user. V49 was documentation-only. V50 adds trace propagation. V51 adds metrics. V52 adds admin API. V53 adds security & audit. V54 adds ERP domain foundation (R5 Phase 1). V55 adds ERP domain extensions (R5 Phase 2).

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
  - **Inventory Module**: Item aggregate (code, name, type, base unit, costing method, standard cost, stock flags); Warehouse aggregate (code, name, type, address, negative stock allowance); KardexEntry aggregate (receipt/issue/adjustment, running balance, cost tracking); CostingEngine (Average, FIFO, LIFO, Standard, Specific costing); Enums (ItemType, ItemStatus, CostingMethod, InventoryTransactionType, WarehouseType); Domain events for all state transitions.
  - **Dimensions Module**: CostCenter aggregate (hierarchical, budget, manager); Project aggregate (dates, budget, manager, customer); CustomDimension aggregate (flexible key-value attributes, hierarchy support); Enums (DimensionType, DimensionStatus); Domain events for all state transitions.
  - **Numbering Module**: NumberSequence aggregate (prefix/suffix, scope, auto-reset, format template, min/max); NumberingScope (Company, Branch, Warehouse, User, Global); NumberingStatus (Active, Inactive, Exhausted); Domain events for number generation, reset, exhaustion.
  - **Tax Module**: TaxCode aggregate (VAT, SalesTax, Withholding, Excise, CustomDuty); Calculation methods (Percentage, FixedAmount, Tiered, Custom); TaxApplication (OnNetAmount, OnGrossAmount, OnQuantity); TaxTier for tiered rates; IsRecoverable flag; Domain events for all state transitions.

## Validation commands
```powershell
dotnet test .\tests\XFramework.Tests\
dotnet test .\tests\XFramework.IntegrationTests\
dotnet build .\XFramework.slnx
```
Confirm actual solution path first. For SQL Server tests, set `XFRAMEWORK_SQLSERVER_TEST_CONNECTION` and verify test output proves the database scenario ran.
