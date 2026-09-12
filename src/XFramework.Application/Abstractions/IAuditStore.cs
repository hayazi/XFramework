using XFramework.Domain.Auditing;

namespace XFramework.Application.Abstractions;

public interface IAuditStore
{
    Task SaveAsync(
        AuditEntry entry,
        CancellationToken cancellationToken = default);
}