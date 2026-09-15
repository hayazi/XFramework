using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using XFramework.Application.Contracts.Events;
using XFramework.Application.Events;

namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqMessageHandler
{
    private readonly IServiceScopeFactory _scopeFactory;

    public RabbitMqMessageHandler(
        IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    private async Task HandleFailureAsync(
        OutboxMessage message,
        string lockId,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var retryCount = message.RetryCount + 1;

        if (!_retryPolicy.ShouldRetry(
                retryCount,
                exception))
        {
            await _repository.MarkFailedAsync(
                message.Id,
                lockId,
                DateTime.UtcNow,
                exception.ToString(),
                cancellationToken);

            return;
        }

        var delay =
            _retryPolicy.GetDelay(retryCount);

        await _repository.MarkFailedAsync(
            message.Id,
            lockId,
            DateTime.UtcNow.Add(delay),
            exception.ToString(),
            cancellationToken);
    }
    public async Task HandleAsync(
        IChannel channel,
        ulong deliveryTag,
        ReadOnlyMemory<byte> body,
        CancellationToken cancellationToken)
    {
        EventEnvelope? envelope = null;

        try
        {
            envelope =
                Deserialize(body);

            var routingKey =
                ExtractRoutingKey(envelope);

            using var scope =
                _scopeFactory.CreateScope();

            var processor =
                scope.ServiceProvider
                    .GetRequiredService<IEventProcessor>();

            await processor.ProcessAsync(
                envelope,
                cancellationToken);

            await channel.BasicAckAsync(
                deliveryTag,
                multiple: false,
                cancellationToken);
        }
        catch (Exception exception)
        {
            await HandleFailureAsync(
                channel,
                deliveryTag,
                envelope,
                exception,
                cancellationToken);
        }
    }
}