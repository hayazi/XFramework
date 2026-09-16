namespace XFramework.Application.Events;

public interface IIdempotencyService
{
    Task<bool> TryBeginProcessingAsync(
        Guid eventId,
        string handlerName,
        string? correlationId = null,
        CancellationToken cancellationToken = default);
}