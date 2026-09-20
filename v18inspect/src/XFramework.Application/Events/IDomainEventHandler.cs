using XFramework.Domain.Events;

namespace XFramework.Application.Events;

public interface IDomainEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    Task HandleAsync(
        TEvent domainEvent,
        CancellationToken cancellationToken = default);
}