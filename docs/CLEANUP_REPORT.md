# XFramework Cleanup & Consolidation Report

This revision consolidates the framework after the first build review.

## Architecture

The solution contains exactly these runtime projects:

- XFramework.Domain
- XFramework.Application.Contracts
- XFramework.Application
- XFramework.EntityFrameworkCore
- XFramework.Infrastructure
- XFramework.Blazor

`XFramework.Core` has been removed from all source and project references.

## Consolidations

- One canonical Domain Entity implementation.
- One canonical `CrudAppService` under `XFramework.Application/Services`.
- One canonical business `XFrameworkDbContext` under `EntityFrameworkCore/Persistence`.
- `EventEnvelope` exists only in `Application.Contracts`.
- Outbox processor contract exists in `Application.Outbox`.
- RabbitMQ implementation exists only in `Infrastructure`.
- ASP.NET `HttpCurrentUser` implementation exists in `Infrastructure.Security`.
- Obsolete incomplete application interceptor implementations were removed from compilation; the application pipeline will be rebuilt coherently in a later step.
- Application service registration uses discovery rather than duplicate registration mechanisms.

## DI lifetime rules

- EF Core DbContexts, repositories, UnitOfWork and EventProcessor: Scoped.
- Event registry, retry policies and RabbitMQ connection/channel infrastructure: Singleton where appropriate.
- RabbitMQ message handler: Scoped and resolved inside a scope per delivery.
- Outbox and RabbitMQ consumers/topology: Hosted services.

## Messaging corrections

- Retry publishing uses the delay-specific retry routing key.
- Retry queues dead-letter back to the module main queue through `module.#`.
- The original event routing key is retained in message headers.
- RabbitMQ connection creation no longer has circular dependencies.

## Important remaining architectural work

This cleanup is intended to remove contradictory/obsolete implementations and restore a coherent buildable baseline. The following are deliberately next-stage work rather than hidden inside this cleanup:

1. Atomic SQL Server outbox claiming with `UPDLOCK/READPAST/ROWLOCK`.
2. Robust duplicate-key detection for idempotency.
3. Complete application-service interceptor/pipeline implementation.
4. Identity business-context UnitOfWork alignment.
5. RabbitMQ integration tests and failure/crash tests.
