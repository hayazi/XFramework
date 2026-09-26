# Application Service Pipeline

Implemented in `XFramework.Application`.

## Execution order

```text
Logging
  ↓
Authorization
  ↓
Validation
  ↓
Unit Of Work
  ↓
Application Service
```

The Castle DynamicProxy adapter is responsible only for converting interface calls into the framework pipeline. Business/application services remain unaware of Castle.

## Main types

- `Interceptors/IApplicationServiceInterceptor.cs`
- `Interceptors/ApplicationServiceInvocationContext.cs`
- `Interceptors/ApplicationServicePipelineInterceptor.cs`
- `Interceptors/LoggingApplicationServiceInterceptor.cs`
- `Interceptors/AuthorizationApplicationServiceInterceptor.cs`
- `Interceptors/ValidationApplicationServiceInterceptor.cs`
- `Interceptors/UnitOfWorkApplicationServiceInterceptor.cs`

## Registration

Application services are discovered automatically, registered as scoped implementation types, and exposed through interface proxies. Validators implementing `IValidator<T>` are also discovered automatically.

## Validation

A service or method opts in with `[Validate]`. `CustomerAppService` is marked with `[Validate]` as the first framework example.

## Unit of Work

- `[ReadOnly]` disables the transactional UoW.
- `[UnitOfWork(...)]` explicitly controls the behavior.
- Without an attribute, mutation methods ending in `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `SaveAsync`, `PostAsync`, or `ApplyAsync` use a transactional UoW.
- Other methods do not automatically open a transaction.

## Important boundary

The pipeline is orchestration/cross-cutting behavior only. Domain rules remain in Domain, persistence remains in EntityFrameworkCore, and external integrations remain in Infrastructure.

## Build verification

The current execution environment does not contain the .NET SDK, so this package was not locally compiled here. Verify with:

```powershell
dotnet build .\src\XFramework.Blazor\
```
