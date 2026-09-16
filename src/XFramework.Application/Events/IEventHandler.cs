using XFramework.Domain.Events;

namespace XFramework.Application.Events;

public interface IEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    Task HandleAsync(
        TEvent @event,
        CancellationToken cancellationToken = default);
}