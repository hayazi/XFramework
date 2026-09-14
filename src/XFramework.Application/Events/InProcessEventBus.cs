using Microsoft.Extensions.DependencyInjection;

namespace XFramework.Application.Events;

public sealed class InProcessEventBus : IEventBus
{
    private readonly IServiceProvider _serviceProvider;

    public InProcessEventBus(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task PublishAsync(
        string eventType,
        string payload,
        CancellationToken cancellationToken = default)
    {
        // Temporary implementation.
        // The real message-broker implementation
        // will be added later.

        await Task.CompletedTask;
    }
}