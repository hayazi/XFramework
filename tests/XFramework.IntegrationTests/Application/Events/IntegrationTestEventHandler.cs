using XFramework.Application.Events;
using XFramework.IntegrationTests.Infrastructure;

namespace XFramework.IntegrationTests.Application.Events;

public sealed class IntegrationTestEventHandler
    : IEventHandler<IntegrationTestEvent>, IDomainEventHandler<IntegrationTestEvent>
{
    private readonly IntegrationTestStore _store;

    public IntegrationTestEventHandler(IntegrationTestStore store)
    {
        _store = store;
    }

    public Task HandleAsync(
        IntegrationTestEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        _store.Record(
            domainEvent.TestId,
            domainEvent.Message);

        return Task.CompletedTask;
    }
}
