using Microsoft.EntityFrameworkCore;
using XFramework.Application.Abstractions;
using XFramework.Domain.Auditing;
using XFramework.EntityFrameworkCore.Persistence;

namespace XFramework.EntityFrameworkCore.Auditing;

public sealed class EfCoreAuditStore(XFrameworkDbContext db) : IAuditStore
{
    public async Task SaveAsync(
        AuditEntry entry,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        db.AuditEntries.Add(entry);
        await db.SaveChangesAsync(cancellationToken);
    }
}