namespace XFramework.Application.Idempotency;

public interface IIdempotencyService
{
    Task<bool> TryBeginProcessingAsync(
        Guid eventId,
        string handlerName,
        string? correlationId = null,
        CancellationToken cancellationToken = default);
}