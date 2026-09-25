using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using XFramework.Application.Contracts.Events;
using XFramework.Application.Events;
using XFramework.Infrastructure.Messaging;

namespace XFramework.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqMessageHandler(
    IEventProcessor processor,
    IEventRetryPolicy retryPolicy,
    IEventRetryPublisher retryPublisher,
    IEventDeadLetterPublisher dlq,
    ILogger<RabbitMqMessageHandler> logger)
{
    public async Task HandleAsync(
        IChannel channel,
        BasicDeliverEventArgs args,
        CancellationToken ct)
    {
        var body = args.Body.ToArray();
        EventEnvelope? env = null;

        try
        {
            env = JsonSerializer.Deserialize<EventEnvelope>(body);
        }
        catch (JsonException ex)
        {
            await dlq.PublishRawAsync(body, args.RoutingKey, ex, ct);
            await channel.BasicAckAsync(args.DeliveryTag, false, ct);
            return;
        }

        if (env is null)
        {
            var ex = new InvalidOperationException("Event envelope is null.");
            await dlq.PublishRawAsync(body, args.RoutingKey, ex, ct);
            await channel.BasicAckAsync(args.DeliveryTag, false, ct);
            return;
        }

        var traceParent = GetHeaderValue(args.BasicProperties.Headers, "traceparent");
        var traceState = GetHeaderValue(args.BasicProperties.Headers, "tracestate");
        var correlationId = GetHeaderValue(args.BasicProperties.Headers, "correlation-id");
        var causationId = GetHeaderValue(args.BasicProperties.Headers, "causation-id");

        ActivityContext parentContext = default;
        if (!string.IsNullOrWhiteSpace(traceParent))
        {
            try
            {
                parentContext = ActivityContext.Parse(traceParent, traceState);
            }
            catch
            {
                // Invalid traceparent format, continue without parent context
            }
        }

        using var activity = MessagingDiagnostics.ActivitySource.StartActivity(
            "event.process",
            ActivityKind.Consumer,
            parentContext);

        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag("messaging.destination", args.RoutingKey);
        activity?.SetTag("messaging.message_id", env.EventId.ToString());
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            activity?.SetTag("messaging.correlation_id", correlationId);
        }
        if (!string.IsNullOrWhiteSpace(causationId))
        {
            activity?.SetTag("messaging.causation_id", causationId);
        }
        activity?.SetTag("messaging.message_retry_count", env.RetryCount);

        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["EventId"] = env.EventId,
            ["EventType"] = env.EventType,
            ["CorrelationId"] = correlationId ?? string.Empty,
            ["CausationId"] = causationId ?? string.Empty,
            ["TraceId"] = activity?.TraceId.ToString() ?? string.Empty,
            ["SpanId"] = activity?.SpanId.ToString() ?? string.Empty
        });

        try
        {
            await processor.ProcessAsync(env, ct);
            await channel.BasicAckAsync(args.DeliveryTag, false, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            if (retryPolicy.ShouldRetry(env.RetryCount, ex))
            {
                await retryPublisher.PublishRetryAsync(env, args.RoutingKey, retryPolicy.GetDelay(env.RetryCount), ct);
                await channel.BasicAckAsync(args.DeliveryTag, false, ct);
                logger.LogWarning(ex, "Event {EventId} scheduled for retry.", env.EventId);
            }
            else
            {
                await dlq.PublishAsync(env, args.RoutingKey, ex, ct);
                await channel.BasicAckAsync(args.DeliveryTag, false, ct);
                logger.LogError(ex, "Event {EventId} moved to DLQ.", env.EventId);
            }
        }
    }

    private static string? GetHeaderValue(IDictionary<string, object?>? headers, string key)
    {
        if (headers is null)
        {
            return null;
        }

        if (headers.TryGetValue(key, out var value) && value is string str)
        {
            return str;
        }

        if (headers.TryGetValue(key, out var byteArray) && byteArray is byte[] bytes)
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        return null;
    }
}
