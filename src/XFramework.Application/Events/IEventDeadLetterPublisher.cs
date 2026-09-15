namespace XFramework.Application.Events;

using XFramework.Application.Contracts.Events;

public interface IEventDeadLetterPublisher
{
    Task PublishAsync(
        EventEnvelope envelope,
        string routingKey,
        Exception exception,
        CancellationToken cancellationToken = default);
}