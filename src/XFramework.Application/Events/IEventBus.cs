namespace XFramework.Application.Events;

public interface IEventBus
{
    Task PublishAsync(
        string eventType,
        string payload,
        CancellationToken cancellationToken = default);
}