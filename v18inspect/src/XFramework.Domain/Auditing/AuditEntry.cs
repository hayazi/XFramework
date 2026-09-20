namespace XFramework.Domain.Auditing;

public sealed class AuditEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string? UserId { get; init; }

    public string? UserName { get; init; }

    public string EntityName { get; init; } = string.Empty;

    public string EntityId { get; init; } = string.Empty;

    public string Action { get; init; } = string.Empty;

    public DateTime Timestamp { get; init; }

    public string? IpAddress { get; init; }

    public string? ChangesJson { get; init; }
}