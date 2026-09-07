# XFramework

A modular application framework for .NET 10 inspired by ABP, built around **DDD**, **Castle DynamicProxy** interceptors, and **Entity Framework Core**. It provides an opinionated layered architecture with automatic application-service discovery, declarative authorization, validation, and unit-of-work behavior.

---

## Projects / Architecture

The solution (`XFramework.slnx`) is split into seven projects following a classic layered DDD layout:

| Project | Role |
|---|---|
| `src/XFramework.Core` | Framework-agnostic building blocks: `Entity<TKey>`, `AggregateRoot`, `Result<T>`, `BusinessException`, and marker interfaces (`IAggregateRoot`, `ISoftDelete`, `ICreationAudited`, `IModificationAudited`, `IConcurrencyAware`). |
| `src/XFramework.Domain` | Domain entities and domain events. Depends only on `XFramework.Core`. Currently contains the `Customer` aggregate. |
| `src/XFramework.Application.Contracts` | Application service **interfaces** and DTOs (`EntityDto`, `PagedAndSortedRequestDto`, `PagedResult<T>`). Also defines the authorization contracts (`IPermissionChecker`, `IPermissionDefinitionProvider`, `IPermissionDefinitionContext`, `PermissionDefinition`). |
| `src/XFramework.Application` | Application service **implementations**, generic `CrudAppService<TEntity, TEntityDto, TKey, TCreateDto, TUpdateDto>`, Castle interceptors (`UnitOfWorkInterceptor`, `ValidationInterceptor`, `AuthorizationInterceptor`), attribute-based conventions (`[Authorize]`, `[UnitOfWork]`, `[Validate]`, `[ReadOnly]`), permission discovery, and `AddXFrameworkApplication(...)` registration. |
| `src/XFramework.EntityFrameworkCore` | EF Core integration. Defines `ERPDbContext`, the generic `EfRepository<TEntity, TKey>`, `EfUnitOfWork`, entity configurations, and `AddXFrameworkEntityFrameworkCore(configuration)`. Targets SQL Server. |
| `src/XFramework.Blazor` | Reference host application: Blazor (Server interactive) app that wires the framework up in `Program.cs` and ships sample pages for `Customers`, `Counter`, `Weather`, etc. |
| `tests/XFramework.Tests` | xUnit test project (`XFramework.Tests.csproj`) referencing `Core`, `Domain`, and `Application`. |

Project reference graph:

```
XFramework.Blazor ──► Application ──► Domain ──► Core
                  └─► Application.Contracts ──► Core
                  └─► EntityFrameworkCore ──► Application
                                           └─► Domain
XFramework.Tests  ──► Core, Domain, Application
```

All projects target `net10.0` with `Nullable` and `ImplicitUsings` enabled.

---

## Key Concepts

### 1. Application Services + Auto-Discovery

Any non-abstract class whose name ends in `AppService` and implements an interface deriving from `IApplicationService` is **automatically discovered** at startup by `ApplicationServiceDiscovery` and registered with the DI container (see `ApplicationServiceRegistration` / `ApplicationServiceProxyExtensions`). The registered runtime instance is a **Castle DynamicProxy** so the framework's interceptors can run transparently around every call.

```csharp
[Authorize("CRM.Customer")]
[UnitOfWork]
[Validate]
public class CustomerAppService
    : CrudAppService<Customer, CustomerDto, Guid, CustomerCreateDto, CustomerUpdateDto>,
      ICustomerAppService
{ ... }
```

### 2. `CrudAppService<TEntity, TEntityDto, TKey, TCreateDto, TUpdateDto>`

A reusable base class providing `GetAsync`, `GetListAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync` plus mapping hooks (`MapToDto`, `MapToEntityAsync`) backed by `IRepository<TEntity, TKey>` and `IUnitOfWork`. See `ICrudAppService<TDto, TKey, TCreate, TUpdate>` and `IReadOnlyAppService`.

### 3. Cross-Cutting Concerns via Attributes

The `XFramework.Application` project ships convention attributes that are read by interceptors and method metadata caches (`ApplicationMethodMetadata`):

| Attribute | Behavior |
|---|---|
| `[Authorize("Permission.Name")]` | `AuthorizationInterceptor` checks each required permission via `IPermissionChecker`; throws `AuthorizationException` if denied. Supports class- and method-level declarations. |
| `[UnitOfWork]` | `UnitOfWorkInterceptor` opens a transaction (or ambient UoW) around method execution. |
| `[Validate]` | `ValidationInterceptor` invokes registered `IValidator<T>` implementations and throws `ValidationException` on failure. |
| `[ReadOnly]` | Marks a method as not requiring a write transaction. |

### 4. Permission System

Permissions are defined in groups through `IPermissionDefinitionProvider` implementations and discovered via `PermissionProviderDiscovery`. The reference provider `CustomerPermissionDefinitionProvider` declares a `CRM` group with `CRM.Customer` and its `View / Create / Edit / Delete` children. The runtime registry is exposed as `PermissionDefinitionRegistry`.

`DefaultPermissionChecker` is the stock implementation of `IPermissionChecker` registered by default; replace it with your own provider to integrate identity/roles.

### 5. Validation

`IValidator<T>` plus `ValidationResult` form a tiny pluggable pipeline. Custom validators are auto-discovered; the reference app uses `CustomerCreateValidator` and `CustomerUpdateValidator` via FluentValidation-style rules.

### 6. Entity Framework Core Integration

`AddXFrameworkEntityFrameworkCore(configuration)` wires up:

- `ERPDbContext` configured for SQL Server (`Microsoft.EntityFrameworkCore.SqlServer` 10.0.11) using `ConnectionStrings:Default`.
- A `XFrameworkDbContext` alias registered as the base context.
- Generic `EfRepository<TEntity, TKey>` bound to `IRepository<TEntity, TKey>`.
- `EfUnitOfWork` bound to `IUnitOfWork`.
- Entity configurations picked up via `ApplyConfigurationsFromAssembly`.
- An initial migration: `20260906142247_InitialCreate`.

---

## Getting Started

### Prerequisites

- .NET SDK **10.0**
- SQL Server (local or reachable instance) — the default connection string points to `Server=localhost;Database=XFrameworkDb`.

### Build & Run

```powershell
# Restore + build
dotnet build XFramework.slnx

# Apply EF Core migrations to the configured database
dotnet ef database update `
  --project src/XFramework.EntityFrameworkCore `
  --startup-project src/XFramework.Blazor

# Run the Blazor reference host
dotnet run --project src/XFramework.Blazor
```

### Wiring It Into Your Own Host

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddXFrameworkApplication(
    typeof(YourAppServiceAssemblyMarker).Assembly);

builder.Services.AddXFrameworkEntityFrameworkCore(
    builder.Configuration);

var app = builder.Build();
// ...
app.Run();
```

To override the default permission checker, register your own `IPermissionChecker` implementation after `AddXFrameworkApplication(...)`.

---

## Sample Module: Customers

The reference module demonstrates every framework feature:

- **Domain:** `Customer : Entity<Guid>` (`src/XFramework.Domain/Customers/Customer.cs`)
- **Contracts:** `ICustomerAppService`, `CustomerDto`, `CustomerCreateDto`, `CustomerUpdateDto`
- **Application:** `CustomerAppService` (CRUD, `[Authorize("CRM.Customer")]`, `[UnitOfWork]`, `[Validate]`), `CustomerPermissionDefinitionProvider`, `CustomerCreateValidator`, `CustomerUpdateValidator`
- **EF Core:** `CustomerConfiguration`
- **UI:** `Components/Pages/Customers.razor`

A `Products` placeholder application service and DTOs are also included.

---

## Testing

```powershell
dotnet test tests/XFramework.Tests/XFramework.Tests.csproj
```

Uses **xUnit** with `coverlet.collector` for coverage.

---

## Technology & Packages

- **.NET 10** (`net10.0`), nullable + implicit usings enabled across all projects.
- **Castle.Core 5.2.1** — DynamicProxy for the interceptor pipeline.
- **Microsoft.Extensions.DependencyInjection.Abstractions 10.0.11**
- **Microsoft.EntityFrameworkCore / .Design / .SqlServer 10.0.11**
- **xUnit 2.9.3**, **Microsoft.NET.Test.Sdk 17.14.1**, **xunit.runner.visualstudio 3.1.4**, **coverlet.collector 6.0.4**

---

## Folder Layout

```
F:\XFramework
├── XFramework.slnx
├── .gitignore
├── src/
│   ├── XFramework.Core/
│   ├── XFramework.Domain/
│   ├── XFramework.Application.Contracts/
│   ├── XFramework.Application/
│   ├── XFramework.EntityFrameworkCore/
│   └── XFramework.Blazor/
└── tests/
    └── XFramework.Tests/
```