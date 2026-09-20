# XFramework

A modular, enterprise-oriented application framework for **.NET 10**, designed around **Domain-Driven Design (DDD)**, **Clean Architecture**, **Castle DynamicProxy**, **Entity Framework Core**, and **Blazor Server / Interactive Server**.

XFramework provides an opinionated foundation for building large-scale ERP, CRM, MIS, and business applications without depending on ABP.IO.

The framework focuses on:

- Domain-Driven Design
- Clean and layered architecture
- Modular application development
- Automatic Application Service discovery
- Generic CRUD Application Services
- Cross-cutting concerns through interceptors
- Permission-based authorization
- Validation
- Unit of Work and transaction management
- Domain Events
- Transactional Outbox
- Event Type Registry and versioning
- Idempotent message processing
- RabbitMQ integration
- Retry and Dead Letter Queue (DLQ)
- Structured logging
- Correlation and tracing
- OpenTelemetry observability
- Background processing
- EF Core persistence
- Blazor Server / Interactive Server UI
- Localization and multilingual applications

---

# 1. Architecture

XFramework follows a layered architecture based on DDD and Clean Architecture principles.

The solution is divided into six main framework projects plus a test project:

```text
XFramework
├── XFramework.Domain
├── XFramework.Application.Contracts
├── XFramework.Application
├── XFramework.EntityFrameworkCore
├── XFramework.Infrastructure
├── XFramework.Blazor
└── XFramework.Tests
```

The primary architectural rule is:

> Business rules belong to the Domain.  
> Application orchestration belongs to Application.  
> Database persistence belongs to EntityFrameworkCore.  
> External and technical infrastructure belongs to Infrastructure.  
> UI and application composition belong to Blazor.

---

# 2. Projects / Architecture

| Project | Responsibility |
|---|---|
| `src/XFramework.Domain` | Pure business domain: entities, aggregate roots, value objects, domain services, specifications, domain events, and domain exceptions. |
| `src/XFramework.Application.Contracts` | Public application contracts: Application Service interfaces, DTOs, paging contracts, authorization contracts, localization contracts, event contracts, and error contracts. |
| `src/XFramework.Application` | Application behavior and orchestration: Application Services, `CrudAppService`, repository/UoW abstractions, application-service pipeline, validation, authorization, event processing, event serialization, and event registry. |
| `src/XFramework.EntityFrameworkCore` | Database and persistence implementation: DbContexts, EF Core repositories, Unit of Work implementation, entity configurations, migrations, Outbox persistence, Idempotency persistence, and EF Core interceptors. |
| `src/XFramework.Infrastructure` | Technical and external infrastructure: RabbitMQ, Redis/cache, Serilog, OpenTelemetry, Hangfire/background jobs, email, file storage, external service integrations, and other infrastructure providers. |
| `src/XFramework.Blazor` | Presentation and Composition Root: Blazor Server / Interactive Server UI, authentication, localization, dependency injection, configuration, application startup, and reference UI. |
| `tests/XFramework.Tests` | Unit and integration tests for Domain, Application, persistence, infrastructure, and framework behavior. |

---

# 3. Dependency Rules

The dependency direction is intentionally controlled.

```text
                         ┌──────────────────────────────┐
                         │      XFramework.Blazor       │
                         │ Presentation / Composition   │
                         └───────────────┬──────────────┘
                                         │
                       ┌─────────────────┼─────────────────┐
                       │                 │                 │
                       ▼                 ▼                 ▼
              Application        EntityFrameworkCore   Infrastructure
                       │                 │                 │
                       ▼                 ▼                 ▼
              Application.Contracts     Application      Application
                       │                 │                 │
                       └─────────────────┼─────────────────┘
                                         ▼
                                XFramework.Domain
```

More specifically:

```text
Domain
  ↑
Application.Contracts
  ↑
Application
  ↑              ↑
EntityFrameworkCore    Infrastructure
  \              /
       XFramework.Blazor
```

The important rule is:

```text
Domain
    ↓
must NOT depend on technical infrastructure.
```

For example, Domain must not reference:

- Entity Framework Core
- RabbitMQ
- Redis
- Serilog
- OpenTelemetry
- Blazor
- HTTP clients
- ASP.NET Core
- SQL Server

---

# 4. Domain Layer

Project:

```text
src/XFramework.Domain
```

The Domain layer contains the actual business model.

Typical contents:

```text
XFramework.Domain
├── Common
│   ├── Entity.cs
│   ├── AggregateRoot.cs
│   ├── ValueObject.cs
│   └── ...
│
├── Entities
├── Aggregates
├── ValueObjects
├── DomainServices
├── Specifications
├── Events
│   ├── IDomainEvent.cs
│   ├── DomainEvent.cs
│   └── IHasDomainEvents.cs
│
└── Exceptions
```

The Domain layer must remain framework-independent.

---

# 5. Domain Events

Domain entities can raise domain events when important business state changes occur.

Example:

```csharp
public interface IDomainEvent
{
    Guid EventId { get; }

    DateTime OccurredOnUtc { get; }

    Guid? CorrelationId { get; }

    Guid? CausationId { get; }
}
```

Base event:

```csharp
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;

    public Guid? CorrelationId { get; init; }

    public Guid? CausationId { get; init; }
}
```

Example business event:

```csharp
public sealed record InventoryDocumentPostedEvent(
    Guid DocumentId,
    string DocumentNumber)
    : DomainEvent;
```

Domain events describe business facts.

They do not know how those events are transported.

For example:

```text
Domain Event
    ↓
Outbox
    ↓
RabbitMQ
```

The Domain itself does not reference RabbitMQ.

---

# 6. Application.Contracts

Project:

```text
src/XFramework.Application.Contracts
```

This project contains contracts shared between the Application layer and presentation/integration layers.

Typical structure:

```text
XFramework.Application.Contracts
├── Application
├── DTOs
├── Paging
├── Authorization
├── Localization
├── Events
│   └── EventEnvelope.cs
└── Errors
    └── ErrorInfo.cs
```

Examples include:

```text
IApplicationService
ICrudAppService
IReadOnlyAppService
PagedResult<T>
PagedAndSortedRequestDto
EntityDto<TKey>
IPermissionChecker
IPermissionDefinitionProvider
ILocalizationService
EventEnvelope
ErrorInfo
```

---

# 7. Application Layer

Project:

```text
src/XFramework.Application
```

The Application layer orchestrates business operations.

Typical structure:

```text
XFramework.Application
├── Services
│   ├── ApplicationService.cs
│   └── CrudAppService.cs
│
├── Repositories
├── UnitOfWork
├── Validation
├── Authorization
├── Events
├── Exceptions
├── Interceptors
├── Permissions
└── DependencyInjection
```

The Application layer may define abstractions for technical services, but it must not contain concrete infrastructure implementations.

For example:

```text
Application
    ↓
IEventBus
    ↓
Infrastructure
    ↓
RabbitMQ
```

Application knows `IEventBus`.

Application does not know `RabbitMQ.Client`.

---

# 8. Application Service Auto-Discovery

Application Services are automatically discovered and registered.

A typical Application Service:

```csharp
[Authorize("CRM.Customer")]
[UnitOfWork]
[Validate]
public class CustomerAppService
    : CrudAppService<
        Customer,
        CustomerDto,
        Guid,
        CustomerCreateDto,
        CustomerUpdateDto>,
      ICustomerAppService
{
}
```

A class can be automatically discovered when it:

1. Is not abstract.
2. Ends with `AppService`.
3. Implements an interface deriving from `IApplicationService`.

The framework automatically registers the service and creates the required proxy so cross-cutting interceptors can execute around the Application Service.

---

# 9. CrudAppService

The canonical reusable CRUD base class is:

```text
XFramework.Application.Services/CrudAppService.cs
```

There must not be a second duplicate `CrudAppService.cs` under:

```text
XFramework.Application/CrudAppService.cs
```

The canonical implementation is:

```text
XFramework.Application
└── Services
    └── CrudAppService.cs
```

The generic service provides common operations such as:

```text
GetAsync
GetListAsync
CreateAsync
UpdateAsync
DeleteAsync
```

Conceptually:

```csharp
CrudAppService<
    TEntity,
    TEntityDto,
    TKey,
    TCreateDto,
    TUpdateDto>
```

The implementation is backed by:

```text
IRepository<TEntity, TKey>
IUnitOfWork
```

Mapping can be customized through mapping hooks such as:

```csharp
MapToDto(...)
MapToEntityAsync(...)
```

---

# 10. Cross-Cutting Concerns

XFramework uses Castle DynamicProxy interceptors for cross-cutting application behavior.

The typical Application Service pipeline is:

```text
Exception Handling
        ↓
Authorization
        ↓
Logging
        ↓
Validation
        ↓
Unit Of Work
        ↓
Application Service
        ↓
Repository
```

Cross-cutting concerns should not be manually duplicated in every Application Service.

---

# 11. Authorization

XFramework uses permission-based authorization.

Contract:

```csharp
public interface IPermissionChecker
{
    Task<bool> IsGrantedAsync(
        string permission,
        CancellationToken cancellationToken = default);

    Task CheckAsync(
        string permission,
        CancellationToken cancellationToken = default);
}
```

Example:

```csharp
[RequiresPermission("Accounting.Documents.Post")]
public Task PostAsync(...)
{
    ...
}
```

Typical ERP permissions may include:

```text
Accounting.Documents.Create
Accounting.Documents.Edit
Accounting.Documents.Post
Accounting.Documents.Delete

Inventory.Receipts.Create
Inventory.Receipts.Post

Inventory.Issues.Create
Inventory.Issues.Post
```

Authorization is handled by the Application pipeline.

---

# 12. Validation

Validation is implemented as a pluggable Application concern.

The framework provides:

```csharp
IValidator<T>
```

and:

```csharp
ValidationResult
```

Validation occurs through the Application Service interceptor pipeline.

Business invariants that are fundamental to the domain remain inside Domain entities and domain services.

---

# 13. Unit Of Work

`IUnitOfWork` is an Application abstraction.

The actual EF Core implementation belongs to:

```text
XFramework.EntityFrameworkCore
```

The Application Service should not manually create transactions.

Instead:

```text
Application Service
        ↓
UnitOfWorkInterceptor
        ↓
IUnitOfWork
        ↓
EF Core
        ↓
Database Transaction
```

This provides centralized transaction management.

---

# 14. Entity Framework Core

Project:

```text
src/XFramework.EntityFrameworkCore
```

This project is responsible specifically for database persistence.

Typical structure:

```text
XFramework.EntityFrameworkCore
├── DbContexts
├── Configurations
├── Repositories
├── UnitOfWork
├── Migrations
├── Outbox
├── Idempotency
└── Interceptors
```

Responsibilities include:

- `XFrameworkDbContext`
- EF Core configurations
- Generic EF repositories
- EF Core Unit of Work
- Database migrations
- Outbox persistence
- Idempotency persistence
- EF Core interceptors

---

# 15. Entity Framework Core Boundary

`EntityFrameworkCore` means:

> Database and persistence infrastructure.

It does not mean:

> All infrastructure.

Therefore external technologies such as RabbitMQ must not be implemented here.

For example:

```text
XFramework.EntityFrameworkCore
    ├── SQL Server
    ├── EF Core
    ├── Repository
    ├── UnitOfWork
    ├── Outbox persistence
    └── Idempotency persistence
```

while:

```text
XFramework.Infrastructure
    ├── RabbitMQ
    ├── Redis
    ├── Serilog
    ├── OpenTelemetry
    ├── Hangfire
    ├── Email
    ├── File Storage
    └── External APIs
```

---

# 16. Infrastructure Layer

Project:

```text
src/XFramework.Infrastructure
```

This project contains technical and external infrastructure implementations.

Typical structure:

```text
XFramework.Infrastructure
├── Messaging
│   └── RabbitMQ
│       ├── RabbitMqOptions.cs
│       ├── RabbitMqConnectionFactory.cs
│       ├── RabbitMqTopology.cs
│       ├── RabbitMqEventBus.cs
│       ├── RabbitMqConsumer.cs
│       └── RabbitMqMessageHandler.cs
│
├── Caching
│   └── Redis
│
├── Logging
│   └── Serilog
│
├── Observability
│   └── OpenTelemetry
│
├── BackgroundJobs
│   └── Hangfire
│
├── Email
├── FileStorage
└── ExternalServices
```

Infrastructure implements abstractions defined by Application or Application.Contracts.

---

# 17. RabbitMQ

RabbitMQ is an Infrastructure concern.

The Application layer depends only on:

```csharp
public interface IEventBus
{
    Task PublishAsync(
        string eventType,
        int eventVersion,
        string payload,
        CancellationToken cancellationToken = default);
}
```

The concrete implementation is:

```text
XFramework.Infrastructure
└── Messaging
    └── RabbitMQ
        └── RabbitMqEventBus.cs
```

The Application layer must never reference:

```text
RabbitMQ.Client
```

directly.

The architectural flow is:

```text
Application
     ↓
IEventBus
     ↓
RabbitMqEventBus
     ↓
RabbitMQ
```

This allows RabbitMQ to be replaced later with another transport without changing Domain or Application business code.

---

# 18. Event Envelope

Integration events are transported using a stable envelope.

```csharp
public sealed record EventEnvelope
{
    public Guid EventId { get; init; }

    public string EventType { get; init; } = null!;

    public int EventVersion { get; init; }

    public string Payload { get; init; } = null!;

    public DateTime OccurredOnUtc { get; init; }

    public Guid? CorrelationId { get; init; }

    public Guid? CausationId { get; init; }

    public int RetryCount { get; init; }

    public string? LastError { get; init; }

    public DateTime? LastAttemptOnUtc { get; init; }
}
```

---

# 19. Event Type Registry

XFramework does not persist assembly-qualified CLR type names as the event contract.

Instead, events use stable names and explicit versions.

Example:

```text
EventType   = Inventory.DocumentPosted
EventVersion = 1
```

The registry maps:

```text
EventType + Version
        ↓
CLR Event Type
```

Example:

```csharp
[EventType("Inventory.DocumentPosted", 1)]
public sealed record InventoryDocumentPostedEvent(
    Guid DocumentId,
    string DocumentNumber)
    : DomainEvent;
```

This allows event contracts to evolve without coupling persisted messages to CLR namespaces or assembly names.

---

# 20. Event Versioning

Events are versioned independently.

Example:

```text
Inventory.DocumentPosted v1
Inventory.DocumentPosted v2
Inventory.DocumentPosted v3
```

Consumers can support multiple versions when necessary.

The event registry is responsible for:

```text
Registration
Lookup
Serialization
Deserialization
Version compatibility
```

This is especially important for messages that may remain in:

```text
Outbox
RabbitMQ
Retry queues
Dead Letter Queue
```

for an extended period.

---

# 21. Transactional Outbox

The Outbox pattern guarantees that a database transaction and its outgoing event are persisted reliably.

The flow is:

```text
Business Operation
       ↓
Domain Event
       ↓
EF Core SaveChanges
       ↓
Outbox Message
       ↓
Same Database Transaction
       ↓
COMMIT
```

The Outbox is persisted in:

```text
XFramework.EntityFrameworkCore
└── Outbox
```

Typical components:

```text
OutboxMessage.cs
OutboxMessageStatus.cs
OutboxMessageConfiguration.cs
DomainEventToOutboxInterceptor.cs
IOutboxRepository.cs
OutboxRepository.cs
OutboxProcessor.cs
OutboxBackgroundService.cs
```

---

# 22. Outbox Message

The Outbox message contains stable event metadata.

Typical fields include:

```text
Id
EventType
EventVersion
Payload
OccurredOnUtc
CreatedOnUtc
Status
RetryCount
NextAttemptOnUtc
ProcessedOnUtc
LastError
CorrelationId
CausationId
AggregateType
AggregateId
LockId
LockedUntilUtc
```

The important status lifecycle is:

```text
Pending
   ↓
Processing
   ↓
Completed
```

or:

```text
Processing
   ↓
Failed
```

Failed messages can become eligible for retry according to the configured retry policy.

---

# 23. Atomic Outbox Claiming

Multiple application instances may run Outbox workers.

Therefore messages must be claimed atomically.

The implementation uses SQL Server locking techniques such as:

```sql
UPDLOCK
READPAST
ROWLOCK
```

Conceptually:

```text
Pending Message
      ↓
Atomic Claim
      ↓
Processing
      ↓
Publish
      ↓
Completed
```

A lease mechanism is used to recover messages when a worker crashes while processing them.

Important fields include:

```text
LockId
LockedUntilUtc
```

---

# 24. Background Outbox Publisher

The Outbox worker periodically searches for eligible messages.

Conceptual flow:

```text
OutboxBackgroundService
        ↓
OutboxProcessor
        ↓
Claim messages
        ↓
Read EventType + Version
        ↓
Create EventEnvelope
        ↓
IEventBus
        ↓
RabbitMQ
        ↓
Mark Completed
```

The worker must not assume that a successful database read means successful message delivery.

---

# 25. Idempotency

Distributed messaging is inherently capable of delivering a message more than once.

XFramework therefore uses idempotency.

The persistent implementation belongs to:

```text
XFramework.EntityFrameworkCore
└── Idempotency
```

Typical components:

```text
ProcessedMessage.cs
ProcessedMessageConfiguration.cs
IdempotencyService.cs
```

The logical key is:

```text
EventId + HandlerName
```

Application abstraction:

```csharp
public interface IIdempotencyService
{
    Task<bool> TryBeginProcessingAsync(
        Guid eventId,
        string handlerName,
        string? correlationId = null,
        CancellationToken cancellationToken = default);
}
```

---

# 26. Idempotent Business Transaction

Idempotency must be part of the same database transaction as the business side effect.

Correct:

```text
BEGIN TRANSACTION

    Idempotency Record
          +
    Accounting Document
          +
    Accounting Lines

COMMIT
```

If the transaction fails:

```text
ROLLBACK
```

Both the idempotency record and business changes are rolled back.

This allows the message to be safely retried.

---

# 27. RabbitMQ Consumer

RabbitMQ is only the transport layer.

The consumer flow is:

```text
RabbitMQ Message
       ↓
Read Envelope
       ↓
Validate
       ↓
Resolve Event Type
       ↓
Deserialize
       ↓
Event Processor
       ↓
Handler
       ↓
Database Transaction
       ↓
COMMIT
       ↓
ACK
```

The message must be acknowledged only after the business transaction succeeds.

```text
Receive
   ↓
Process
   ↓
DB Transaction
   ↓
COMMIT
   ↓
ACK
```

---

# 28. Retry Policy

Not every failure should be retried.

Retryable failures may include:

- Temporary database connectivity problems
- Network timeouts
- Temporary external service failures
- Transient infrastructure failures
- Deadlocks

Non-retryable failures may include:

- Unknown event type
- Unsupported event version
- Invalid event payload
- Invalid business state
- Missing accounting mapping
- Business rule violation

The Application abstraction is:

```csharp
public interface IEventRetryPolicy
{
    bool IsRetryable(Exception exception);

    bool ShouldRetry(
        int retryCount,
        Exception exception);

    TimeSpan GetDelay(int retryCount);
}
```

---

# 29. Retry Queues

A typical retry schedule can be:

```text
Attempt 1 → 5 seconds
Attempt 2 → 30 seconds
Attempt 3 → 2 minutes
Attempt 4 → 10 minutes
Attempt 5 → 30 minutes
```

After the maximum retry count:

```text
RabbitMQ
    ↓
Dead Letter Queue
```

Example queues:

```text
rgre.accounting.events
rgre.inventory.events

rgre.accounting.events.retry.5s
rgre.accounting.events.retry.30s
rgre.accounting.events.retry.2m
rgre.accounting.events.retry.10m
rgre.accounting.events.retry.30m

rgre.accounting.events.dlq
```

---

# 30. Dead Letter Queue

Messages that cannot be successfully processed after the configured retry policy are moved to the DLQ.

The DLQ provides:

- Failure inspection
- Operational troubleshooting
- Manual recovery
- Controlled replay
- Production monitoring

A DLQ message should retain:

```text
EventId
EventType
EventVersion
CorrelationId
CausationId
RetryCount
LastError
Original Payload
```

---

# 31. Crash Safety

The messaging architecture must handle this important scenario:

```text
Handler
   ↓
DB Transaction
   ↓
COMMIT
   ↓
Process crashes
   ↓
RabbitMQ does not receive ACK
   ↓
Message is redelivered
```

The second delivery is detected by:

```text
EventId + HandlerName
```

The idempotency mechanism prevents duplicate business effects.

Then:

```text
ACK
```

can safely be sent.

---

# 32. Observability

XFramework propagates correlation information through the complete operation.

Conceptually:

```text
CorrelationId
      ↓
TraceId
      ↓
EventId
      ↓
Outbox
      ↓
RabbitMQ
      ↓
Consumer
      ↓
Handler
      ↓
Database Transaction
```

Important diagnostic fields include:

```text
CorrelationId
CausationId
EventId
EventType
EventVersion
TraceId
HandlerName
RetryCount
```

---

# 33. Logging

Logging is an Infrastructure concern.

The Application layer can use standard abstractions such as:

```csharp
ILogger<T>
```

while concrete logging infrastructure can be configured through:

```text
XFramework.Infrastructure
└── Logging
    └── Serilog
```

Logs should support structured properties such as:

```text
EventId
CorrelationId
CausationId
UserId
HandlerName
EntityId
Operation
```

---

# 34. OpenTelemetry

Observability infrastructure belongs to:

```text
XFramework.Infrastructure
└── Observability
```

OpenTelemetry can be used for:

- Distributed tracing
- Metrics
- Application diagnostics
- Database instrumentation
- HTTP instrumentation
- Messaging diagnostics

The objective is to trace a business operation across:

```text
Blazor
   ↓
Application Service
   ↓
Database
   ↓
Outbox
   ↓
RabbitMQ
   ↓
Consumer
   ↓
Handler
   ↓
Database
```

---

# 35. Authentication and Identity

Authentication and authorization are separated from the business domain model.

The framework can use ASP.NET Core Identity as the authentication mechanism.

A typical identity user can be represented by:

```csharp
public sealed class XFrameworkIdentityUser
    : IdentityUser<Guid>
{
    public Guid ApplicationUserId { get; set; }
}
```

The business domain should not contain password or authentication implementation details.

---

# 36. Current User

Application code accesses the authenticated user through an abstraction.

```csharp
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    string? UserId { get; }

    string? UserName { get; }

    IReadOnlyCollection<string> Roles { get; }

    bool IsInRole(string role);
}
```

This prevents Domain and Application business logic from depending directly on HTTP or Blazor-specific APIs.

---

# 37. Localization

Localization is designed for multilingual enterprise applications.

The framework supports:

```text
fa
en
```

and can be extended with additional cultures such as:

```text
ar
```

Application contract:

```csharp
public interface ILocalizationService
{
    string Get(
        string key,
        string? defaultValue = null);

    string Get(
        string key,
        params object[] arguments);
}
```

The Blazor layer is responsible for UI localization and RTL/LTR presentation.

---

# 38. Persian / Jalali Calendar

The Domain model should use standard .NET date/time types.

For example:

```csharp
DateTime
DateTimeOffset
```

Jalali/Persian calendar conversion is a UI/presentation concern.

Therefore:

```text
Domain
    ↓
Gregorian/UTC DateTime
    ↓
Blazor UI
    ↓
Jalali Calendar
```

This prevents business logic from becoming coupled to a specific calendar presentation.

---

# 39. Blazor Project

Project:

```text
src/XFramework.Blazor
```

Responsibilities:

- Blazor Server / Interactive Server
- UI components
- Authentication
- Authorization UI
- Localization
- RTL/LTR
- Dependency Injection
- Configuration
- Application startup
- Composition Root

The Blazor project is where concrete implementations are composed.

Example:

```csharp
builder.Services.AddXFrameworkApplication(
    typeof(YourAppServiceAssemblyMarker).Assembly);

builder.Services.AddXFrameworkEntityFrameworkCore(
    builder.Configuration);

builder.Services.AddXFrameworkInfrastructure(
    builder.Configuration);
```

---

# 40. Composition Root

`XFramework.Blazor` is the Composition Root.

It connects abstractions to implementations:

```text
IRepository
      ↓
EF Repository

IUnitOfWork
      ↓
EF UnitOfWork

IEventBus
      ↓
RabbitMqEventBus

ILogging
      ↓
Serilog

IObservability
      ↓
OpenTelemetry
```

The infrastructure implementations must not depend on Blazor.

---

# 41. Reference Application

The Blazor project can contain reference/sample modules demonstrating framework capabilities.

Example:

```text
Customers
Products
```

A sample Customer module can demonstrate:

```text
Domain
    Customer

Contracts
    ICustomerAppService
    CustomerDto
    CustomerCreateDto
    CustomerUpdateDto

Application
    CustomerAppService
    CustomerPermissionDefinitionProvider
    CustomerCreateValidator
    CustomerUpdateValidator

EntityFrameworkCore
    CustomerConfiguration

Blazor
    Customers.razor
```

---

# 42. Application Module Pattern

A business module should follow the same layered structure.

Example:

```text
Accounting
├── Domain
├── Application.Contracts
├── Application
├── EntityFrameworkCore
├── Infrastructure
└── Blazor
```

The module should keep business rules in Domain and orchestration in Application.

---

# 43. Recommended ERP Event Flow

For an ERP such as RGRE.ERP, a typical operational flow is:

```text
Inventory Document
        ↓
Inventory Transaction
        ↓
Inventory Kardex
        ↓
Costing Engine
        ↓
Domain / Integration Event
        ↓
Transactional Outbox
        ↓
RabbitMQ
        ↓
Accounting Event Handler
        ↓
Accounting Document
        ↓
Accounting Lines
```

This keeps Inventory and Accounting decoupled.

Inventory quantity and costing must not depend on reconstructing stock from accounting voucher lines.

---

# 44. Infrastructure vs Persistence

This distinction is fundamental in XFramework.

### EntityFrameworkCore

Responsible for:

```text
SQL Server
EF Core
DbContext
Repositories
UnitOfWork
Migrations
Outbox persistence
Idempotency persistence
EF interceptors
```

### Infrastructure

Responsible for:

```text
RabbitMQ
Redis
Serilog
OpenTelemetry
Hangfire
Email
File Storage
External APIs
Other technical providers
```

Therefore:

```text
RabbitMQ ❌ EntityFrameworkCore
RabbitMQ ✅ Infrastructure
```

and:

```text
Outbox table persistence ✅ EntityFrameworkCore
Outbox publishing to RabbitMQ ✅ Infrastructure
```

---

# 45. Getting Started

## Prerequisites

- .NET SDK 10
- SQL Server
- Visual Studio 2022 or later
- RabbitMQ for messaging features
- Docker Desktop is recommended for local infrastructure services

---

# 46. Build

Restore and build the solution:

```powershell
dotnet restore XFramework.slnx

dotnet build XFramework.slnx
```

---

# 47. Database Migration

Apply EF Core migrations:

```powershell
dotnet ef database update `
  --project src/XFramework.EntityFrameworkCore `
  --startup-project src/XFramework.Blazor
```

---

# 48. Run the Blazor Host

```powershell
dotnet run `
  --project src/XFramework.Blazor
```

---

# 49. RabbitMQ

For local development, RabbitMQ can be run using Docker.

The application should receive RabbitMQ configuration from application configuration rather than hard-coding connection details.

Example configuration concept:

```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  }
}
```

Production credentials must be supplied securely.

---

# 50. Testing

Test project:

```text
tests/XFramework.Tests
```

Run:

```powershell
dotnet test tests/XFramework.Tests/XFramework.Tests.csproj
```

Tests should cover:

```text
Domain
Application
Authorization
Validation
UnitOfWork
Outbox
Idempotency
Event Registry
Event Versioning
RabbitMQ integration
Retry policy
Application Service discovery
```

---

# 51. Technology Stack

The core technology stack includes:

| Technology | Purpose |
|---|---|
| .NET 10 | Runtime and framework |
| ASP.NET Core | Application hosting |
| Blazor Server / Interactive Server | UI |
| C# | Primary programming language |
| Entity Framework Core 10 | Persistence |
| SQL Server | Relational database |
| Castle DynamicProxy | Application Service interception |
| RabbitMQ | Asynchronous messaging |
| Redis | Distributed caching |
| Serilog | Structured logging |
| OpenTelemetry | Observability |
| Hangfire | Background jobs |
| xUnit | Testing |
| Radzen Blazor Free | UI components |

---

# 52. Design Principles

XFramework follows these principles:

### 1. Domain First

Business rules belong to Domain.

### 2. No Business Logic in Database

Business behavior should not be hidden inside SQL stored procedures or database triggers unless there is a deliberate technical reason.

### 3. Application Services Orchestrate

Application Services coordinate operations but should not become large business-logic classes.

### 4. Infrastructure Is Replaceable

Technical providers such as RabbitMQ, Redis, or logging frameworks should be replaceable.

### 5. Persistence Is Separate

EF Core is responsible for database persistence, not external messaging.

### 6. Events Are Versioned

Persisted and transported events use stable event names and explicit versions.

### 7. Messages Are Idempotent

Consumers must tolerate duplicate delivery.

### 8. Transactions Are Explicitly Controlled

Business side effects and idempotency records should participate in the same transaction.

### 9. Cross-Cutting Concerns Are Centralized

Authorization, validation, logging, and Unit of Work should not be duplicated in every Application Service.

### 10. Presentation Does Not Own Business Rules

Blazor is responsible for presentation, not business logic.

---

# 53. Folder Layout

The expected solution layout is:

```text
F:\XFramework
│
├── XFramework.slnx
├── .gitignore
│
├── src/
│   │
│   ├── XFramework.Domain/
│   │   ├── Common/
│   │   ├── Entities/
│   │   ├── Aggregates/
│   │   ├── ValueObjects/
│   │   ├── DomainServices/
│   │   ├── Specifications/
│   │   ├── Events/
│   │   └── Exceptions/
│   │
│   ├── XFramework.Application.Contracts/
│   │   ├── Application/
│   │   ├── DTOs/
│   │   ├── Paging/
│   │   ├── Authorization/
│   │   ├── Localization/
│   │   ├── Events/
│   │   └── Errors/
│   │
│   ├── XFramework.Application/
│   │   ├── Services/
│   │   │   ├── ApplicationService.cs
│   │   │   └── CrudAppService.cs
│   │   ├── Repositories/
│   │   ├── UnitOfWork/
│   │   ├── Validation/
│   │   ├── Authorization/
│   │   ├── Events/
│   │   ├── Exceptions/
│   │   ├── Interceptors/
│   │   ├── Permissions/
│   │   └── DependencyInjection/
│   │
│   ├── XFramework.EntityFrameworkCore/
│   │   ├── DbContexts/
│   │   ├── Configurations/
│   │   ├── Repositories/
│   │   ├── UnitOfWork/
│   │   ├── Migrations/
│   │   ├── Outbox/
│   │   ├── Idempotency/
│   │   └── Interceptors/
│   │
│   ├── XFramework.Infrastructure/
│   │   ├── Messaging/
│   │   │   └── RabbitMQ/
│   │   ├── Caching/
│   │   │   └── Redis/
│   │   ├── Logging/
│   │   │   └── Serilog/
│   │   ├── Observability/
│   │   │   └── OpenTelemetry/
│   │   ├── BackgroundJobs/
│   │   │   └── Hangfire/
│   │   ├── Email/
│   │   ├── FileStorage/
│   │   └── ExternalServices/
│   │
│   └── XFramework.Blazor/
│       ├── Components/
│       ├── Pages/
│       ├── Authentication/
│       ├── Localization/
│       ├── wwwroot/
│       ├── Program.cs
│       └── appsettings.json
│
└── tests/
    └── XFramework.Tests/
```

---

# 54. Final Architecture

The final XFramework architecture is:

```text
                         ┌──────────────────────────┐
                         │    XFramework.Blazor     │
                         │                          │
                         │ UI + Auth + Localization │
                         │ Configuration + DI       │
                         └────────────┬─────────────┘
                                      │
                 ┌────────────────────┼────────────────────┐
                 │                    │                    │
                 ▼                    ▼                    ▼
       ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐
       │   Application    │ │ EntityFramework  │ │  Infrastructure  │
       │                  │ │      Core        │ │                  │
       │ Services         │ │                  │ │ RabbitMQ         │
       │ Interceptors     │ │ DbContext        │ │ Redis            │
       │ Validation       │ │ Repository       │ │ Serilog          │
       │ Authorization    │ │ UnitOfWork       │ │ OpenTelemetry    │
       │ Event Registry   │ │ Outbox           │ │ Hangfire         │
       │ Event Processing │ │ Idempotency      │ │ Email            │
       └────────┬─────────┘ └────────┬─────────┘ │ File Storage     │
                │                    │           │ External APIs    │
                └────────────┬───────┴───────────┴──────────────────┘
                             │
                             ▼
                   ┌─────────────────────┐
                   │ Application.Contracts│
                   └──────────┬──────────┘
                              │
                              ▼
                   ┌─────────────────────┐
                   │  XFramework.Domain  │
                   │                     │
                   │ Entities            │
                   │ Aggregates           │
                   │ Value Objects        │
                   │ Domain Services      │
                   │ Domain Events        │
                   │ Business Rules       │
                   └─────────────────────┘
```

The architectural boundary can be summarized as:

```text
DOMAIN
  = Business

APPLICATION
  = Orchestration

APPLICATION.CONTRACTS
  = Public Contracts

ENTITYFRAMEWORKCORE
  = Database / Persistence

INFRASTRUCTURE
  = External / Technical Infrastructure

BLAZOR
  = Presentation / Composition Root
```

---

# 55. Project Status

The framework is being developed incrementally.

Current architectural capabilities include:

```text
[✓] DDD foundation
[✓] Layered architecture
[✓] Application Service contracts
[✓] Generic CrudAppService
[✓] Application Service auto-discovery
[✓] Castle DynamicProxy pipeline
[✓] Authorization
[✓] Validation
[✓] Unit of Work
[✓] Domain Events
[✓] Transactional Outbox
[✓] Atomic Outbox Claiming
[✓] Idempotency
[✓] Event Type Registry
[✓] Event Versioning
[✓] RabbitMQ Event Bus architecture
[✓] RabbitMQ Consumer architecture
[✓] Retry Policy
[✓] Dead Letter Queue architecture
[✓] Correlation / Causation
[✓] Observability architecture
[✓] Infrastructure project
[ ] Complete production hardening
[ ] Full integration test suite
[ ] Complete Identity implementation
[ ] Complete Redis implementation
[ ] Complete Hangfire integration
[ ] Complete OpenTelemetry implementation
[ ] ERP modules
```

---

# 56. Long-Term Goal

XFramework is intended to be the reusable technical foundation for enterprise applications such as:

```text
RGRE.ERP
RGRE.CRM
RGRE.MIS
```

The framework should remain generic while business-specific functionality belongs to the application built on top of it.

For example:

```text
XFramework
      │
      └── Technical / Application Foundation
                  │
                  ▼
              RGRE.ERP
                  │
        ┌─────────┼─────────┐
        ▼         ▼         ▼
    Accounting  Inventory   Sales
        │         │         │
        └─────────┼─────────┘
                  ▼
             Reporting
```

The objective is to build a production-grade enterprise framework first and then use it as the foundation for the next-generation ERP/MIS platform.