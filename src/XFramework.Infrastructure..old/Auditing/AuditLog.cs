namespace XFramework.Infrastructure.Auditing;

public sealed class AuditLog
{
    public Guid Id { get; set; }

    public string? UserId { get; set; }

    public string? UserName { get; set; }

    public string EntityName { get; set; } = string.Empty;

    public string EntityId { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }

    public string? IpAddress { get; set; }

    public string? ChangesJson { get; set; }
}