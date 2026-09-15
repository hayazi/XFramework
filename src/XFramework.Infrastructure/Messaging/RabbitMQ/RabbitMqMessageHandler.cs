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
        IChannel channel,
        ulong deliveryTag,
        EventEnvelope? envelope,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (envelope is null)
        {
            await channel.BasicNackAsync(
                deliveryTag,
                multiple: false,
                requeue: false,
                cancellationToken);

            return;
        }

        using var scope =
            _scopeFactory.CreateScope();

        var retryPolicy =
            scope.ServiceProvider
                .GetRequiredService<IEventRetryPolicy>();

        var retryPublisher =
            scope.ServiceProvider
                .GetRequiredService<IEventRetryPublisher>();

        var deadLetterPublisher =
            scope.ServiceProvider
                .GetRequiredService<IEventDeadLetterPublisher>();

        var routingKey =
            GetRoutingKey(envelope);

        if (retryPolicy.ShouldRetry(
                envelope.RetryCount,
                exception))
        {
            var delay =
                retryPolicy.GetDelay(
                    envelope.RetryCount);

            await retryPublisher.PublishRetryAsync(
                envelope,
                routingKey,
                delay,
                cancellationToken);

            await channel.BasicAckAsync(
                deliveryTag,
                multiple: false,
                cancellationToken);

            return;
        }

        await deadLetterPublisher.PublishAsync(
            envelope,
            routingKey,
            exception,
            cancellationToken);

        await channel.BasicAckAsync(
            deliveryTag,
            multiple: false,
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