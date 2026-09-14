using Microsoft.EntityFrameworkCore;
using XFramework.Application.Idempotency;

namespace XFramework.EntityFrameworkCore.Idempotency;

public sealed class IdempotencyService
    : IIdempotencyService
{
    private readonly XFrameworkDbContext _dbContext;

    public IdempotencyService(
        XFrameworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> TryBeginProcessingAsync(
        Guid eventId,
        string handlerName,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var entity = new ProcessedMessage
        {
            EventId = eventId,
            HandlerName = handlerName,
            ProcessedOnUtc = DateTime.UtcNow,
            CorrelationId = correlationId
        };

        _dbContext.ProcessedMessages.Add(entity);

        try
        {
            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return true;
        }
        catch (DbUpdateException)
        {
            _dbContext.Entry(entity).State =
                EntityState.Detached;

            return false;
        }
    }
}