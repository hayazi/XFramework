using XFramework.Application.Contracts.Events;

namespace XFramework.Application.Events;

/// <summary>
/// Minimal in-process event bus useful for tests and local scenarios.
/// RabbitMQ is the production transport implementation.
/// </summary>
public sealed class InProcessEventBus : IEventBus
{
    private readonly IEventProcessor _processor;

    public InProcessEventBus(IEventProcessor processor)
    {
        _processor = processor;
    }

    public Task PublishAsync(
        EventEnvelope envelope,
        CancellationToken cancellationToken = default) =>
        _processor.ProcessAsync(envelope, cancellationToken);
}
