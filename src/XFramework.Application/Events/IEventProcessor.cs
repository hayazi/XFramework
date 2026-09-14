using XFramework.Domain.Events;

namespace XFramework.Application.Events;

public interface IEventProcessor
{
    Task ProcessAsync(
        IDomainEvent domainEvent,
        CancellationToken cancellationToken = default);
}