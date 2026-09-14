using Microsoft.Extensions.DependencyInjection;
using XFramework.Domain.Events;

namespace XFramework.Application.Events;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> events,
        CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in events)
        {
            var eventType = domainEvent.GetType();

            var handlerType =
                typeof(IDomainEventHandler<>)
                    .MakeGenericType(eventType);

            var handlers = _serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                await ((dynamic)handler).HandleAsync(
                    (dynamic)domainEvent,
                    cancellationToken);
            }
        }
    }
}