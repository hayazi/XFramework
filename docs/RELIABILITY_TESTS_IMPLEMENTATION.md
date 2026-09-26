# Reliability Tests - Stage 5

Stage 5 introduces a dedicated `XFramework.Tests` project without requiring external infrastructure for the first test layer.

## Scope

The tests lock down the framework contracts that are easy to regress while RabbitMQ infrastructure evolves:

- stable module-scoped routing keys
- exact retry routing keys
- retry delay names
- retryable/non-retryable exception classification
- retry exhaustion at the configured limit
- RabbitMQ development defaults

## Why the tests are split this way

The first layer must be deterministic and runnable with `dotnet test` on a developer workstation. Infrastructure-backed tests are intentionally a second layer because they require RabbitMQ/SQL Server lifecycle management and should not make every local build dependent on Docker.

## Expected workflow

```text
dotnet build .\\src\\XFramework.Blazor\\
dotnet test .\\tests\\XFramework.Tests\\
```

After this layer is green, the next reliability work should add environment-backed integration tests for the actual broker and database concurrency behavior.
