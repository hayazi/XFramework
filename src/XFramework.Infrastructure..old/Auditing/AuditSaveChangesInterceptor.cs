using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using XFramework.Application.Abstractions;
using XFramework.Domain.Auditing;

namespace XFramework.Infrastructure.Auditing;

public sealed class AuditSaveChangesInterceptor(
    ICurrentUser currentUser)
    : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser = currentUser;

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        CreateAuditEntries(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CreateAuditEntries(eventData.Context);

        return base.SavingChangesAsync(
            eventData,
            result,
            cancellationToken);
    }

    private void CreateAuditEntries(DbContext? context)
    {
        if (context is null)
            return;

        var entries = context.ChangeTracker
            .Entries()
            .Where(x =>
                x.State == EntityState.Added ||
                x.State == EntityState.Modified ||
                x.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            if (entry.Entity is AuditLog)
                continue;

            CreateAuditEntry(context, entry);
        }
    }

    private void CreateAuditEntry(
        DbContext context,
        EntityEntry entry)
    {
        var audit = new AuditEntry
        {
            UserId = _currentUser.UserId,
            UserName = _currentUser.UserName,
            EntityName = entry.Metadata.ClrType.Name,
            EntityId = GetEntityId(entry),
            Action = GetAction(entry.State),
            Timestamp = DateTime.UtcNow,
            ChangesJson = SerializeChanges(entry)
        };

        var auditLog = new AuditLog
        {
            Id = audit.Id,
            UserId = audit.UserId,
            UserName = audit.UserName,
            EntityName = audit.EntityName,
            EntityId = audit.EntityId,
            Action = audit.Action,
            Timestamp = audit.Timestamp,
            IpAddress = null,
            ChangesJson = audit.ChangesJson
        };

        context.Set<AuditLog>().Add(auditLog);
    }

    private static string GetAction(EntityState state)
    {
        return state switch
        {
            EntityState.Added => "Created",
            EntityState.Modified => "Updated",
            EntityState.Deleted => "Deleted",
            _ => "Unknown"
        };
    }

    private static string GetEntityId(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();

        if (key is null)
            return string.Empty;

        return string.Join(
            ",",
            key.Properties.Select(property =>
                entry.Property(property.Name).CurrentValue?.ToString()));
    }

    private static string? SerializeChanges(EntityEntry entry)
    {
        var changes = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (entry.State == EntityState.Added)
            {
                changes[property.Metadata.Name] =
                    new
                    {
                        OldValue = (object?)null,
                        NewValue = property.CurrentValue
                    };
            }
            else if (entry.State == EntityState.Deleted)
            {
                changes[property.Metadata.Name] =
                    new
                    {
                        OldValue = property.OriginalValue,
                        NewValue = (object?)null
                    };
            }
            else if (entry.State == EntityState.Modified &&
                     property.IsModified)
            {
                changes[property.Metadata.Name] =
                    new
                    {
                        OldValue = property.OriginalValue,
                        NewValue = property.CurrentValue
                    };
            }
        }

        return changes.Count == 0
            ? null
            : JsonSerializer.Serialize(changes);
    }
}