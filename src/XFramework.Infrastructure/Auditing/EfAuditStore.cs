using XFramework.Application.Abstractions;
using XFramework.Domain.Auditing;
using XFramework.Infrastructure.Persistence;

namespace XFramework.Infrastructure.Auditing;

public sealed class EfAuditStore(
    XFrameworkDbContext db) : IAuditStore
{
    public async Task SaveAsync(
        AuditEntry entry,
        CancellationToken cancellationToken = default)
    {
        var audit = new AuditLog
        {
            Id = entry.Id,
            UserId = entry.UserId,
            UserName = entry.UserName,
            EntityName = entry.EntityName,
            EntityId = entry.EntityId,
            Action = entry.Action,
            Timestamp = entry.Timestamp,
            IpAddress = entry.IpAddress,
            ChangesJson = entry.ChangesJson
        };

        await db.AuditLogs.AddAsync(
            audit,
            cancellationToken);
    }
}