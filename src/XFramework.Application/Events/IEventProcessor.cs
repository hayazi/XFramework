using XFramework.Application.Contracts.Events;

namespace XFramework.Application.Events;

public interface IEventProcessor
{
    Task ProcessAsync(
        EventEnvelope envelope,
        CancellationToken cancellationToken = default);
}