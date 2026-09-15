using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using XFramework.Application.Contracts.Events;
using XFramework.Application.Events;

namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqMessageHandler
{
    private readonly IEventProcessor _eventProcessor;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RabbitMqMessageHandler(
        IEventProcessor eventProcessor)
    {
        _eventProcessor = eventProcessor;
    }

    public async Task HandleAsync(
        IChannel channel,
        ulong deliveryTag,
        ReadOnlyMemory<byte> body,
        CancellationToken cancellationToken)
    {
        try
        {
            var json = Encoding.UTF8.GetString(body.Span);

            var envelope =
                JsonSerializer.Deserialize<EventEnvelope>(
                    json,
                    JsonOptions);

            if (envelope is null)
            {
                throw new InvalidOperationException(
                    "RabbitMQ message envelope is invalid.");
            }

            await _eventProcessor.ProcessAsync(
                envelope,
                cancellationToken);

            await channel.BasicAckAsync(
                deliveryTag,
                multiple: false,
                cancellationToken);
        }
        catch
        {
            await channel.BasicNackAsync(
                deliveryTag,
                multiple: false,
                requeue: false,
                cancellationToken);

            throw;
        }
    }
}