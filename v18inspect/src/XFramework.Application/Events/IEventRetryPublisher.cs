using XFramework.Application.Contracts.Events;

namespace XFramework.Application.Events;

public interface IEventRetryPublisher
{
    Task PublishRetryAsync(
        EventEnvelope envelope,
        string routingKey,
        TimeSpan delay,
        CancellationToken cancellationToken = default);
}