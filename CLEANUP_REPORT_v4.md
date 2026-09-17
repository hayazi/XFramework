# XFramework Cleanup v4

This version addresses the build errors reported from v3 and consolidates several architectural issues discovered during the build.

## Fixes

1. `PermissionDefinition.AddChild` is explicitly public and null-safe.
2. `AuthorizationInterceptor` uses the existing `AuthorizationException` contract and has safer invocation-target handling.
3. `RabbitMqDeadLetterPublisher` now implements `IEventDeadLetterPublisher.PublishRawAsync(...)` for malformed/raw messages.
4. `EfCoreRepositoryExtensions` now references `XFramework.Application.Abstractions.IRepository` rather than the obsolete Contracts.Services namespace.
5. EF Core files now explicitly import `XFramework.EntityFrameworkCore.Persistence` for `XFrameworkDbContext`.
6. Identity domain DbSets were renamed to `ApplicationUsers`, `ApplicationRoles`, `ApplicationUserRoles`, `ApplicationUserPermissions`, `ApplicationPermissions`, and `ApplicationRolePermissions` so they do not collide with ASP.NET Identity's inherited `Users`, `Roles`, etc.
7. `PermissionChecker`, `EfCorePermissionRepository`, and `EfCoreRoleRepository` were updated to use the renamed application-identity DbSets.
8. `PermissionChecker` now uses `IsEnabled`, matching the domain `Permission` model.
9. The Outbox hosted background worker was moved from `XFramework.EntityFrameworkCore` to `XFramework.Infrastructure`, because hosting/background execution is infrastructure rather than persistence.
10. `OutboxOptions` was moved to `XFramework.Application.Outbox` as the shared configuration contract consumed by the EF Outbox processor and Infrastructure hosted worker.
11. Outbox options registration and hosted-worker registration were moved to Infrastructure DI.

## Important next architectural work

The following are intentionally not hidden by this cleanup and should be handled after the solution builds cleanly:

- Atomic SQL Server Outbox claim using UPDLOCK/READPAST/ROWLOCK.
- Race-safe idempotency using the unique key plus duplicate-key handling.
- Terminal Outbox Failed/DLQ behavior after max retries.
- Identity-specific UnitOfWork for Role CRUD so Identity changes are committed by the correct DbContext.
- Rebuild the application-service pipeline coherently (authorization, validation, logging, UoW) after the build is green.
