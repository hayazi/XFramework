using Microsoft.Extensions.DependencyInjection;
using XFramework.Domain.Events;

namespace XFramework.Application.Events;

public sealed class DomainEventDispatcher
    : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(
        IDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var eventType = domainEvent.GetType();

        var handlerType =
            typeof(IDomainEventHandler<>)
                .MakeGenericType(eventType);

        var handlers =
            _serviceProvider
                .GetServices(handlerType);

        foreach (var handler in handlers)
        {
            var method =
                handlerType.GetMethod(nameof(
                    IDomainEventHandler<IDomainEvent>
                        .HandleAsync));

            if (method is null)
            {
                throw new InvalidOperationException(
                    $"Handler method was not found for " +
                    $"event '{eventType.FullName}'.");
            }

            var task =
                (Task?)method.Invoke(
                    handler,
                    new object?[]
                    {
                        domainEvent,
                        cancellationToken
                    });

            if (task is not null)
                await task;
        }
    }
}