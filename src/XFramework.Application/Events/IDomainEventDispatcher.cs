using XFramework.Domain.Events;

namespace XFramework.Application.Events;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> events,
        CancellationToken cancellationToken = default);
}