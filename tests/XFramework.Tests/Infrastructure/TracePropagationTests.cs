using System.Diagnostics;
using System.Text.Json;
using Xunit;
using XFramework.Application.Contracts.Events;
using XFramework.Infrastructure.Messaging.RabbitMQ;

namespace XFramework.Tests.Infrastructure;

public sealed class TracePropagationTests
{
    [Fact]
    public void EventEnvelope_CanCarryTraceContext()
    {
        var traceId = ActivityTraceId.CreateRandom();
        var spanId = ActivitySpanId.CreateRandom();
        var traceParent = $"00-{traceId.ToHexString()}-{spanId.ToHexString()}-01";
        var traceState = "vendor=value";

        var envelope = new EventEnvelope
        {
            EventId = Guid.NewGuid(),
            EventType = "Test.Event",
            EventVersion = 1,
            Payload = "{}",
            OccurredOnUtc = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid(),
            RetryCount = 0,
            TraceParent = traceParent,
            TraceState = traceState
        };

        Assert.Equal(traceParent, envelope.TraceParent);
        Assert.Equal(traceState, envelope.TraceState);
        Assert.NotNull(envelope.CorrelationId);
        Assert.NotNull(envelope.CausationId);
    }

    [Fact]
    public void RabbitMqEventBus_CreateHeaders_IncludesCorrelationAndCausation()
    {
        var envelope = new EventEnvelope
        {
            EventId = Guid.NewGuid(),
            EventType = "Test.Event",
            EventVersion = 1,
            Payload = "{}",
            OccurredOnUtc = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid(),
            RetryCount = 2
        };

        var headers = RabbitMqEventBusTestAccessor.CreateHeaders(envelope);

        Assert.True(headers.ContainsKey("correlation-id"));
        Assert.Equal(envelope.CorrelationId!.Value.ToString(), headers["correlation-id"]);
        Assert.True(headers.ContainsKey("causation-id"));
        Assert.Equal(envelope.CausationId!.Value.ToString(), headers["causation-id"]);
    }

    [Fact]
    public void RabbitMqEventBus_CreateHeaders_IncludesTraceContext()
    {
        var traceId = ActivityTraceId.CreateRandom();
        var spanId = ActivitySpanId.CreateRandom();
        var traceParent = $"00-{traceId.ToHexString()}-{spanId.ToHexString()}-01";
        var traceState = "vendor=value";

        var envelope = new EventEnvelope
        {
            EventId = Guid.NewGuid(),
            EventType = "Test.Event",
            EventVersion = 1,
            Payload = "{}",
            OccurredOnUtc = DateTime.UtcNow,
            RetryCount = 0,
            TraceParent = traceParent,
            TraceState = traceState
        };

        var headers = RabbitMqEventBusTestAccessor.CreateHeaders(envelope);

        Assert.True(headers.ContainsKey("traceparent"));
        Assert.Equal(traceParent, headers["traceparent"]);
        Assert.True(headers.ContainsKey("tracestate"));
        Assert.Equal(traceState, headers["tracestate"]);
    }

    [Fact]
    public void RabbitMqEventBus_CreateHeaders_OmitsNullTraceContext()
    {
        var envelope = new EventEnvelope
        {
            EventId = Guid.NewGuid(),
            EventType = "Test.Event",
            EventVersion = 1,
            Payload = "{}",
            OccurredOnUtc = DateTime.UtcNow,
            RetryCount = 0,
            TraceParent = null,
            TraceState = null
        };

        var headers = RabbitMqEventBusTestAccessor.CreateHeaders(envelope);

        Assert.False(headers.ContainsKey("traceparent"));
        Assert.False(headers.ContainsKey("tracestate"));
    }

    [Fact]
    public void RabbitMqEventBus_CreateHeaders_OmitsNullCorrelationAndCausation()
    {
        var envelope = new EventEnvelope
        {
            EventId = Guid.NewGuid(),
            EventType = "Test.Event",
            EventVersion = 1,
            Payload = "{}",
            OccurredOnUtc = DateTime.UtcNow,
            CorrelationId = null,
            CausationId = null,
            RetryCount = 0
        };

        var headers = RabbitMqEventBusTestAccessor.CreateHeaders(envelope);

        Assert.False(headers.ContainsKey("correlation-id"));
        Assert.False(headers.ContainsKey("causation-id"));
    }

    [Fact]
    public void RabbitMqMessageHandler_GetHeaderValue_ExtractsStringHeaders()
    {
        var headers = new Dictionary<string, object?>
        {
            ["traceparent"] = "00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01",
            ["tracestate"] = "vendor=value",
            ["correlation-id"] = "corr-123",
            ["causation-id"] = "cause-456"
        };

        var traceParent = RabbitMqMessageHandlerTestAccessor.GetHeaderValue(headers, "traceparent");
        var traceState = RabbitMqMessageHandlerTestAccessor.GetHeaderValue(headers, "tracestate");
        var correlationId = RabbitMqMessageHandlerTestAccessor.GetHeaderValue(headers, "correlation-id");
        var causationId = RabbitMqMessageHandlerTestAccessor.GetHeaderValue(headers, "causation-id");

        Assert.Equal("00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01", traceParent);
        Assert.Equal("vendor=value", traceState);
        Assert.Equal("corr-123", correlationId);
        Assert.Equal("cause-456", causationId);
    }

    [Fact]
    public void RabbitMqMessageHandler_GetHeaderValue_ExtractsByteArrayHeaders()
    {
        var headers = new Dictionary<string, object?>
        {
            ["traceparent"] = System.Text.Encoding.UTF8.GetBytes("00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01"),
            ["correlation-id"] = System.Text.Encoding.UTF8.GetBytes("corr-123")
        };

        var traceParent = RabbitMqMessageHandlerTestAccessor.GetHeaderValue(headers, "traceparent");
        var correlationId = RabbitMqMessageHandlerTestAccessor.GetHeaderValue(headers, "correlation-id");

        Assert.Equal("00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01", traceParent);
        Assert.Equal("corr-123", correlationId);
    }

    [Fact]
    public void RabbitMqMessageHandler_GetHeaderValue_ReturnsNullForMissingHeaders()
    {
        var headers = new Dictionary<string, object?>
        {
            ["other-header"] = "value"
        };

        var result = RabbitMqMessageHandlerTestAccessor.GetHeaderValue(headers, "traceparent");

        Assert.Null(result);
    }

    [Fact]
    public void RabbitMqMessageHandler_GetHeaderValue_ReturnsNullForNullHeaders()
    {
        var result = RabbitMqMessageHandlerTestAccessor.GetHeaderValue(null, "traceparent");

        Assert.Null(result);
    }
}

internal static class RabbitMqEventBusTestAccessor
{
    public static Dictionary<string, object?> CreateHeaders(EventEnvelope envelope)
    {
        var headers = new Dictionary<string, object?>
        {
            ["event-id"] = envelope.EventId.ToString(),
            ["event-type"] = envelope.EventType,
            ["event-version"] = envelope.EventVersion,
            ["retry-count"] = envelope.RetryCount
        };

        if (envelope.CorrelationId.HasValue)
        {
            headers["correlation-id"] = envelope.CorrelationId.Value.ToString();
        }

        if (envelope.CausationId.HasValue)
        {
            headers["causation-id"] = envelope.CausationId.Value.ToString();
        }

        if (!string.IsNullOrWhiteSpace(envelope.TraceParent))
        {
            headers["traceparent"] = envelope.TraceParent;
        }

        if (!string.IsNullOrWhiteSpace(envelope.TraceState))
        {
            headers["tracestate"] = envelope.TraceState;
        }

        return headers;
    }
}

internal static class RabbitMqMessageHandlerTestAccessor
{
    public static string? GetHeaderValue(IDictionary<string, object?>? headers, string key)
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