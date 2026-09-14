using XFramework.Domain.Events;

namespace XFramework.Application.Events;

public interface IEventSerializer
{
    string Serialize(IDomainEvent domainEvent);

    IDomainEvent Deserialize(
        string eventType,
        int eventVersion,
        string payload);
}