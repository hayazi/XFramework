namespace XFramework.EntityFrameworkCore.Idempotency;

public sealed class ProcessedMessage
{
    public Guid EventId { get; set; }

    public string HandlerName { get; set; } = null!;

    public DateTime ProcessedOnUtc { get; set; }

    public string? CorrelationId { get; set; }
}