using Microsoft.EntityFrameworkCore;
using XFramework.Application.Events;
using XFramework.EntityFrameworkCore.Persistence;

namespace XFramework.EntityFrameworkCore.Idempotency;

/// <summary>
/// SQL Server implementation of the idempotency store.
/// The insert is protected by key-range locking so concurrent consumers
/// cannot both win the same EventId + HandlerName key.
/// </summary>
public sealed class IdempotencyService(XFrameworkDbContext db) : IIdempotencyService
{
    public async Task<bool> TryBeginProcessingAsync(
        Guid eventId,
        string handlerName,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(handlerName);

        var processedOnUtc = DateTime.UtcNow;

        // HOLDLOCK gives SERIALIZABLE semantics for the key lookup and,
        // together with the primary-key index, prevents two concurrent
        // consumers from inserting the same idempotency key.
        // UPDLOCK makes the intent explicit and avoids a read-then-insert race.
        var rowsInserted = await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO [ProcessedMessages]
            (
                [EventId],
                [HandlerName],
                [ProcessedOnUtc],
                [CorrelationId]
            )
            SELECT
                {eventId},
                {handlerName},
                {processedOnUtc},
                {correlationId}
            WHERE NOT EXISTS
            (
                SELECT 1
                FROM [ProcessedMessages] WITH (UPDLOCK, HOLDLOCK)
                WHERE [EventId] = {eventId}
                  AND [HandlerName] = {handlerName}
            );
            """, cancellationToken);

        return rowsInserted == 1;
    }
}
