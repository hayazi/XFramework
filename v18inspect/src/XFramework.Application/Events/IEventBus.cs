using XFramework.Application.Contracts.Events;

namespace XFramework.Application.Events;

public interface IEventBus
{
    Task PublishAsync(
        EventEnvelope envelope,
        CancellationToken cancellationToken = default);
}